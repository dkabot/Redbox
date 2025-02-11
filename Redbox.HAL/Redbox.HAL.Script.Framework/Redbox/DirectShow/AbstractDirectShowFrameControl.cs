using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow
{
    public abstract class AbstractDirectShowFrameControl : IVideoSource, ISampleGrabberCB
    {
        private static readonly Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities =
            new Dictionary<string, VideoCapabilities[]>();

        private static readonly Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs =
            new Dictionary<string, VideoInput[]>();

        protected readonly string ControlMoniker;
        protected readonly bool Debug;
        private readonly string deviceMoniker;
        protected readonly int GrabWaitTime;
        private readonly ManualResetEvent StartEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent StopEvent = new ManualResetEvent(false);
        private readonly object sync = new object();
        protected string bitmapToSave;
        private VideoInput crossbarVideoInput = VideoInput.Default;
        private VideoInput[] crossbarVideoInputs;
        private Size FrameSize;
        private int framesReceived;
        private bool? isCrossbarAvailable;
        private int m_request;
        private bool needToSetVideoInput;
        private object sourceObject;
        private Thread thread;
        private VideoCapabilities[] videoCapabilities;

        protected AbstractDirectShowFrameControl(string deviceMoniker, int snapTimeout, bool debug)
        {
            this.deviceMoniker = deviceMoniker;
            GrabWaitTime = snapTimeout;
            Debug = debug;
            ControlMoniker = GetType().Name;
        }

        public VideoCapabilities VideoResolution { get; set; }

        public VideoCapabilities[] VideoCapabilities
        {
            get
            {
                if (videoCapabilities == null)
                {
                    lock (cacheVideoCapabilities)
                    {
                        if (!string.IsNullOrEmpty(deviceMoniker))
                            if (cacheVideoCapabilities.ContainsKey(deviceMoniker))
                                videoCapabilities = cacheVideoCapabilities[deviceMoniker];
                    }

                    if (videoCapabilities == null)
                    {
                        if (!IsRunning)
                            WorkerThread(false);
                        else
                            for (var index = 0; index < 500 && videoCapabilities == null; ++index)
                                Thread.Sleep(10);
                    }
                }

                return videoCapabilities == null ? new VideoCapabilities[0] : videoCapabilities;
            }
        }

        protected abstract bool SnapRequested { get; }

        public int SampleCB(double sampleTime, IntPtr sample)
        {
            return 0;
        }

        public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
        {
            if (!SnapRequested)
                return 0;
            if (Debug)
            {
                LogHelper.Instance.Log("[{0}]: BufferCB called ( request = {1} ).", ControlMoniker, m_request);
                if (File.Exists(bitmapToSave))
                    LogHelper.Instance.Log("{0} WARNING: the file {1} already exists.", ControlMoniker, bitmapToSave);
            }

            ++framesReceived;
            using (var bitmap = new Bitmap(FrameSize.Width, FrameSize.Height, PixelFormat.Format24bppRgb))
            {
                var bitmapdata = bitmap.LockBits(new Rectangle(0, 0, FrameSize.Width, FrameSize.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                var stride1 = bitmapdata.Stride;
                var stride2 = bitmapdata.Stride;
                var dst = (byte*)((IntPtr)((long)bitmapdata.Scan0 + stride2 * (FrameSize.Height - 1))).ToPointer();
                var pointer = (byte*)buffer.ToPointer();
                for (var index = 0; index < FrameSize.Height; ++index)
                {
                    Win32.memcpy(dst, pointer, stride1);
                    dst -= stride2;
                    pointer += stride1;
                }

                bitmap.UnlockBits(bitmapdata);
                bitmap.Save(bitmapToSave);
            }

            OnImageCaptured();
            return 0;
        }

        public event NewFrameEventHandler NewFrame
        {
            add { }
            remove { }
        }

        public event VideoSourceErrorEventHandler VideoSourceError
        {
            add { }
            remove { }
        }

        public event PlayingFinishedEventHandler PlayingFinished;

        public virtual string Source => deviceMoniker;

        public int FramesReceived
        {
            get
            {
                var framesReceived = this.framesReceived;
                this.framesReceived = 0;
                return framesReceived;
            }
        }

        public bool IsRunning
        {
            get
            {
                if (thread != null)
                {
                    if (!thread.Join(0))
                        return true;
                    Free();
                }

                return false;
            }
        }

        public bool Start()
        {
            if (IsRunning)
                return true;
            if (string.IsNullOrEmpty(deviceMoniker))
            {
                LogHelper.Instance.Log("[{0}] Video source is not specified - cannot start graph.", ControlMoniker);
                return false;
            }

            framesReceived = 0;
            isCrossbarAvailable = new bool?();
            needToSetVideoInput = true;
            lock (sync)
            {
                thread = new Thread(WorkerThread);
                thread.Name = deviceMoniker;
                thread.Start();
            }

            if (StartEvent.WaitOne(10000))
                return true;
            LogHelper.Instance.Log("timed out waiting for start event.");
            return false;
        }

        public void SignalToStop()
        {
            SignalToStop(true);
        }

        public void WaitForStop()
        {
            SignalToStop();
        }

        public void Stop()
        {
            SignalToStop();
        }

        public static AbstractDirectShowFrameControl GetControl(
            FrameControls control,
            string moniker,
            int grab,
            bool debug)
        {
            if (control == FrameControls.Unknown)
                throw new ArgumentException(nameof(control));
            return FrameControls.Modern == control
                ? new FrameControlModern(moniker, grab, debug)
                : (AbstractDirectShowFrameControl)new FrameControlLegacy(moniker, grab, debug);
        }

        public void SignalToStop(bool wait)
        {
            if (thread == null)
                return;
            StopEvent.Set();
            if (wait)
                thread.Join();
            Free();
        }

        public bool SimulateTrigger(string fileName, int waitTime)
        {
            bitmapToSave = fileName;
            ++m_request;
            if (Debug)
                LogHelper.Instance.Log("[{0}] SnapRequest {1}", ControlMoniker, m_request);
            try
            {
                return OnSimulateTrigger(fileName, waitTime);
            }
            finally
            {
                bitmapToSave = null;
            }
        }

        protected abstract bool OnSimulateTrigger(string file, int captureWait);

        protected abstract void OnImageCaptured();

        protected virtual bool OnGraphEvent()
        {
            return true;
        }

        protected abstract void OnFree();

        private void Free()
        {
            thread = null;
            StopEvent.Close();
            StartEvent.Close();
            try
            {
                OnFree();
            }
            catch
            {
            }
        }

        private void WorkerThread()
        {
            WorkerThread(true);
        }

        private void WorkerThread(bool runGraph)
        {
            var reason = ReasonToFinishPlaying.StoppedByUser;
            var o1 = (object)null;
            var o2 = (object)null;
            var o3 = (object)null;
            var retInterface = (object)null;
            var captureGraphBuilder2 = (ICaptureGraphBuilder2)null;
            var filterGraph2_1 = (IFilterGraph2)null;
            var baseFilter1 = (IBaseFilter)null;
            var baseFilter2 = (IBaseFilter)null;
            var sampleGrabber1 = (ISampleGrabber)null;
            var mediaControl1 = (IMediaControl)null;
            var mediaEventEx1 = (IMediaEventEx)null;
            var crossbar = (IAMCrossbar)null;
            try
            {
                var typeFromClsid1 = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
                if (typeFromClsid1 == null)
                {
                    LogHelper.Instance.Log("[{0}] Failed creating capture graph builder", ControlMoniker);
                    return;
                }

                o1 = Activator.CreateInstance(typeFromClsid1);
                var graphBuilder = (ICaptureGraphBuilder2)o1;
                var typeFromClsid2 = Type.GetTypeFromCLSID(Clsid.FilterGraph);
                if (typeFromClsid2 == null)
                {
                    LogHelper.Instance.Log("[{0}]Failed creating filter graph", ControlMoniker);
                    return;
                }

                o2 = Activator.CreateInstance(typeFromClsid2);
                var filterGraph2_2 = (IFilterGraph2)o2;
                graphBuilder.SetFiltergraph((IGraphBuilder)filterGraph2_2);
                sourceObject = FilterInfo.CreateFilter(deviceMoniker);
                if (sourceObject == null)
                {
                    LogHelper.Instance.Log("[{0}] Failed creating device object for moniker", ControlMoniker);
                    return;
                }

                var sourceObject1 = (IBaseFilter)sourceObject;
                try
                {
                    var sourceObject2 = (IAMVideoControl)sourceObject;
                }
                catch
                {
                }

                var typeFromClsid3 = Type.GetTypeFromCLSID(Clsid.SampleGrabber);
                if (typeFromClsid3 == null)
                {
                    LogHelper.Instance.Log("[{0}] Failed creating sample grabber", ControlMoniker);
                    return;
                }

                o3 = Activator.CreateInstance(typeFromClsid3);
                var sampleGrabber2 = (ISampleGrabber)o3;
                var baseFilter3 = (IBaseFilter)o3;
                filterGraph2_2.AddFilter(sourceObject1, "source");
                filterGraph2_2.AddFilter(baseFilter3, "grabber_video");
                var mediaType = new AMMediaType();
                mediaType.MajorType = MediaType.Video;
                mediaType.SubType = MediaSubType.RGB24;
                sampleGrabber2.SetMediaType(mediaType);
                graphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, sourceObject1,
                    typeof(IAMCrossbar).GUID, out retInterface);
                if (retInterface != null)
                    crossbar = (IAMCrossbar)retInterface;
                isCrossbarAvailable = crossbar != null;
                crossbarVideoInputs = ColletCrossbarVideoInputs(crossbar);
                sampleGrabber2.SetBufferSamples(false);
                sampleGrabber2.SetOneShot(false);
                sampleGrabber2.SetCallback(this, 1);
                GetPinCapabilitiesAndConfigureSizeAndRate(graphBuilder, sourceObject1, PinCategory.Capture,
                    VideoResolution, ref videoCapabilities);
                lock (cacheVideoCapabilities)
                {
                    if (videoCapabilities != null)
                        if (!cacheVideoCapabilities.ContainsKey(deviceMoniker))
                            cacheVideoCapabilities.Add(deviceMoniker, videoCapabilities);
                }

                if (runGraph)
                {
                    graphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, sourceObject1, null, baseFilter3);
                    if (sampleGrabber2.GetConnectedMediaType(mediaType) == 0)
                    {
                        var structure =
                            (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                        FrameSize = new Size(structure.BmiHeader.Width, structure.BmiHeader.Height);
                        mediaType.Dispose();
                    }

                    var mediaControl2 = (IMediaControl)o2;
                    var mediaEventEx2 = (IMediaEventEx)o2;
                    mediaControl2.Run();
                    StartEvent.Set();
                    LogHelper.Instance.Log("[{0}] graph is running; avg rate = {1}.", ControlMoniker,
                        VideoResolution.AverageFrameRate);
                    do
                    {
                        DsEvCode lEventCode;
                        IntPtr lParam1;
                        IntPtr lParam2;
                        if (mediaEventEx2 != null &&
                            mediaEventEx2.GetEvent(out lEventCode, out lParam1, out lParam2, 0) == 0U)
                        {
                            mediaEventEx2.FreeEventParams(lEventCode, lParam1, lParam2);
                            if (Debug)
                                LogHelper.Instance.Log("[{0}]Event: {1}", ControlMoniker, lEventCode.ToString());
                            if (lEventCode == DsEvCode.DeviceLost)
                            {
                                reason = ReasonToFinishPlaying.DeviceLost;
                                break;
                            }
                        }

                        if (needToSetVideoInput)
                        {
                            needToSetVideoInput = false;
                            if (isCrossbarAvailable.Value)
                            {
                                SetCurrentCrossbarInput(crossbar, crossbarVideoInput);
                                crossbarVideoInput = GetCurrentCrossbarInput(crossbar);
                            }
                        }

                        if (!OnGraphEvent() && mediaEventEx2 != null &&
                            mediaEventEx2.GetEvent(out lEventCode, out lParam1, out lParam2, 0) == 0U)
                        {
                            mediaEventEx2.FreeEventParams(lEventCode, lParam1, lParam2);
                            LogHelper.Instance.Log(">> [{0}] Event: {1}", ControlMoniker, lEventCode.ToString());
                        }
                    } while (!StopEvent.WaitOne(100, false));

                    mediaControl2.Stop();
                    LogHelper.Instance.Log("[{0}] Shutdown statistics: there were {1} frames received via video.",
                        ControlMoniker, framesReceived);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                captureGraphBuilder2 = null;
                filterGraph2_1 = null;
                baseFilter1 = null;
                mediaControl1 = null;
                mediaEventEx1 = null;
                baseFilter2 = null;
                sampleGrabber1 = null;
                if (o2 != null)
                    Marshal.ReleaseComObject(o2);
                if (sourceObject != null)
                {
                    Marshal.ReleaseComObject(sourceObject);
                    sourceObject = null;
                }

                if (o3 != null)
                    Marshal.ReleaseComObject(o3);
                if (o1 != null)
                    Marshal.ReleaseComObject(o1);
                if (retInterface != null)
                    Marshal.ReleaseComObject(retInterface);
            }

            if (PlayingFinished == null)
                return;
            PlayingFinished(this, reason);
        }

        private void SetResolution(IAMStreamConfig streamConfig, VideoCapabilities resolution)
        {
            if (resolution == null)
                return;
            var count = 0;
            var size = 0;
            var mediaType = (AMMediaType)null;
            var streamConfigCaps = new VideoStreamConfigCaps();
            streamConfig.GetNumberOfCapabilities(out count, out size);
            for (var index = 0; index < count; ++index)
                try
                {
                    var videoCapabilities = new VideoCapabilities(streamConfig, index);
                    if (resolution == videoCapabilities)
                        if (streamConfig.GetStreamCaps(index, out mediaType, streamConfigCaps) == 0)
                            break;
                }
                catch
                {
                }

            if (mediaType == null)
                return;
            streamConfig.SetFormat(mediaType);
            mediaType.Dispose();
        }

        private void GetPinCapabilitiesAndConfigureSizeAndRate(
            ICaptureGraphBuilder2 graphBuilder,
            IBaseFilter baseFilter,
            Guid pinCategory,
            VideoCapabilities resolutionToSet,
            ref VideoCapabilities[] capabilities)
        {
            object retInterface;
            graphBuilder.FindInterface(pinCategory, MediaType.Video, baseFilter, typeof(IAMStreamConfig).GUID,
                out retInterface);
            if (retInterface != null)
            {
                var amStreamConfig = (IAMStreamConfig)null;
                try
                {
                    amStreamConfig = (IAMStreamConfig)retInterface;
                }
                catch (InvalidCastException ex)
                {
                }

                if (amStreamConfig != null)
                {
                    if (capabilities == null)
                        try
                        {
                            capabilities = DirectShow.VideoCapabilities.FromStreamConfig(amStreamConfig);
                        }
                        catch
                        {
                        }

                    if (resolutionToSet != null)
                        SetResolution(amStreamConfig, resolutionToSet);
                }
            }

            if (capabilities != null)
                return;
            capabilities = new VideoCapabilities[0];
        }

        private VideoInput[] ColletCrossbarVideoInputs(IAMCrossbar crossbar)
        {
            lock (cacheCrossbarVideoInputs)
            {
                if (cacheCrossbarVideoInputs.ContainsKey(deviceMoniker))
                    return cacheCrossbarVideoInputs[deviceMoniker];
                var videoInputList = new List<VideoInput>();
                int inputPinCount;
                if (crossbar != null && crossbar.get_PinCounts(out _, out inputPinCount) == 0)
                    for (var index = 0; index < inputPinCount; ++index)
                    {
                        PhysicalConnectorType physicalType;
                        if (crossbar.get_CrossbarPinInfo(true, index, out _, out physicalType) == 0 &&
                            physicalType < PhysicalConnectorType.AudioTuner)
                            videoInputList.Add(new VideoInput(index, physicalType));
                    }

                var array = new VideoInput[videoInputList.Count];
                videoInputList.CopyTo(array);
                cacheCrossbarVideoInputs.Add(deviceMoniker, array);
                return array;
            }
        }

        private VideoInput GetCurrentCrossbarInput(IAMCrossbar crossbar)
        {
            var currentCrossbarInput = VideoInput.Default;
            int outputPinCount;
            if (crossbar.get_PinCounts(out outputPinCount, out _) == 0)
            {
                var outputPinIndex = -1;
                int pinIndexRelated;
                for (var pinIndex = 0; pinIndex < outputPinCount; ++pinIndex)
                {
                    PhysicalConnectorType physicalType;
                    if (crossbar.get_CrossbarPinInfo(false, pinIndex, out pinIndexRelated, out physicalType) == 0 &&
                        physicalType == PhysicalConnectorType.VideoDecoder)
                    {
                        outputPinIndex = pinIndex;
                        break;
                    }
                }

                int inputPinIndex;
                if (outputPinIndex != -1 && crossbar.get_IsRoutedTo(outputPinIndex, out inputPinIndex) == 0)
                {
                    PhysicalConnectorType physicalType;
                    crossbar.get_CrossbarPinInfo(true, inputPinIndex, out pinIndexRelated, out physicalType);
                    currentCrossbarInput = new VideoInput(inputPinIndex, physicalType);
                }
            }

            return currentCrossbarInput;
        }

        private void SetCurrentCrossbarInput(IAMCrossbar crossbar, VideoInput videoInput)
        {
            int outputPinCount;
            int inputPinCount;
            if (videoInput.Type == PhysicalConnectorType.Default ||
                crossbar.get_PinCounts(out outputPinCount, out inputPinCount) != 0)
                return;
            var outputPinIndex = -1;
            var inputPinIndex = -1;
            int pinIndexRelated;
            PhysicalConnectorType physicalType;
            for (var pinIndex = 0; pinIndex < outputPinCount; ++pinIndex)
                if (crossbar.get_CrossbarPinInfo(false, pinIndex, out pinIndexRelated, out physicalType) == 0 &&
                    physicalType == PhysicalConnectorType.VideoDecoder)
                {
                    outputPinIndex = pinIndex;
                    break;
                }

            for (var pinIndex = 0; pinIndex < inputPinCount; ++pinIndex)
                if (crossbar.get_CrossbarPinInfo(true, pinIndex, out pinIndexRelated, out physicalType) == 0 &&
                    physicalType == videoInput.Type && pinIndex == videoInput.Index)
                {
                    inputPinIndex = pinIndex;
                    break;
                }

            if (inputPinIndex == -1 || outputPinIndex == -1 || crossbar.CanRoute(outputPinIndex, inputPinIndex) != 0)
                return;
            crossbar.Route(outputPinIndex, inputPinIndex);
        }
    }
}
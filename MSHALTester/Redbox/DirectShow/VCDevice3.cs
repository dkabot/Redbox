using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public class VCDevice3 : IVideoSource
{
    private static readonly Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities = new();
    private static readonly Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs = new();
    private readonly bool Debug;
    private readonly string deviceMoniker;
    private readonly int GrabWaitTime;
    private readonly object m_listLock = new();
    private readonly List<Bitmap> ReceivedFrames = new();
    private readonly ManualResetEvent StartEvent = new(false);
    private readonly object sync = new();
    private VideoInput crossbarVideoInput = VideoInput.Default;
    private VideoInput[] crossbarVideoInputs;
    private int framesReceived;
    private bool? isCrossbarAvailable;
    private bool needToSetVideoInput;
    private ManualResetEvent stopEvent;
    private Thread thread;
    private VideoCapabilities[] videoCapabilities;

    public VCDevice3(string deviceMoniker, int snapTimeout, bool debug)
    {
        this.deviceMoniker = deviceMoniker;
        GrabWaitTime = snapTimeout;
        Debug = debug;
    }

    public VideoInput CrossbarVideoInput
    {
        get => crossbarVideoInput;
        set
        {
            needToSetVideoInput = true;
            crossbarVideoInput = value;
        }
    }

    public VideoInput[] AvailableCrossbarVideoInputs
    {
        get
        {
            if (crossbarVideoInputs == null)
            {
                lock (cacheCrossbarVideoInputs)
                {
                    if (!string.IsNullOrEmpty(deviceMoniker))
                        if (cacheCrossbarVideoInputs.ContainsKey(deviceMoniker))
                            crossbarVideoInputs = cacheCrossbarVideoInputs[deviceMoniker];
                }

                if (crossbarVideoInputs == null)
                {
                    if (!IsRunning)
                        WorkerThread(false);
                    else
                        for (var index = 0; index < 500 && crossbarVideoInputs == null; ++index)
                            Thread.Sleep(10);
                }
            }

            return crossbarVideoInputs == null ? new VideoInput[0] : crossbarVideoInputs;
        }
    }

    [Obsolete]
    public Size DesiredFrameSize
    {
        get => Size.Empty;
        set { }
    }

    [Obsolete]
    public Size DesiredSnapshotSize
    {
        get => Size.Empty;
        set { }
    }

    [Obsolete]
    public int DesiredFrameRate
    {
        get => 0;
        set { }
    }

    public VideoCapabilities VideoResolution { get; set; }

    public VideoCapabilities SnapshotResolution { get; set; }

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

    public object SourceObject { get; private set; }

    private bool Triggered { get; set; }

    public event NewFrameEventHandler NewFrame
    {
        add { }
        remove { }
    }

    public event VideoSourceErrorEventHandler VideoSourceError;

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
            LogHelper.Instance.Log("[DirectShow] Video source is not specified - cannot start graph.");
            return false;
        }

        framesReceived = 0;
        isCrossbarAvailable = new bool?();
        needToSetVideoInput = true;
        stopEvent = new ManualResetEvent(false);
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
        if (thread == null)
            return;
        stopEvent.Set();
    }

    public void WaitForStop()
    {
        if (thread == null)
            return;
        thread.Join();
        Free();
    }

    public void Stop()
    {
        if (!IsRunning)
            return;
        thread.Abort();
        WaitForStop();
    }

    public event NewFrameEventHandler SnapshotFrame;

    private void Free()
    {
        thread = null;
        stopEvent.Close();
        stopEvent = null;
    }

    public bool CheckIfCrossbarAvailable()
    {
        lock (sync)
        {
            if (!isCrossbarAvailable.HasValue)
            {
                if (!IsRunning)
                    WorkerThread(false);
                else
                    for (var index = 0; index < 500 && !isCrossbarAvailable.HasValue; ++index)
                        Thread.Sleep(10);
            }

            return isCrossbarAvailable.HasValue && isCrossbarAvailable.Value;
        }
    }

    public void SimulateTrigger()
    {
        if (Debug)
            LogHelper.Instance.Log("[DirectShow] Simulate trigger.");
        Triggered = true;
    }

    private void WorkerThread()
    {
        WorkerThread(true);
    }

    private void WorkerThread(bool runGraph)
    {
        var reason = ReasonToFinishPlaying.StoppedByUser;
        var callback = new SampleGrabber(OnNewFrame);
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
                LogHelper.Instance.Log("[DirectShow] Failed creating capture graph builder");
                return;
            }

            o1 = Activator.CreateInstance(typeFromClsid1);
            var graphBuilder = (ICaptureGraphBuilder2)o1;
            var typeFromClsid2 = Type.GetTypeFromCLSID(Clsid.FilterGraph);
            if (typeFromClsid2 == null)
            {
                LogHelper.Instance.Log("Failed creating filter graph");
                return;
            }

            o2 = Activator.CreateInstance(typeFromClsid2);
            var filterGraph2_2 = (IFilterGraph2)o2;
            graphBuilder.SetFiltergraph((IGraphBuilder)filterGraph2_2);
            SourceObject = FilterInfo.CreateFilter(deviceMoniker);
            if (SourceObject == null)
            {
                LogHelper.Instance.Log("[DirectShow] Failed creating device object for moniker");
                return;
            }

            var sourceObject1 = (IBaseFilter)SourceObject;
            try
            {
                var sourceObject2 = (IAMVideoControl)SourceObject;
            }
            catch
            {
            }

            var typeFromClsid3 = Type.GetTypeFromCLSID(Clsid.SampleGrabber);
            if (typeFromClsid3 == null)
            {
                LogHelper.Instance.Log("[DirectShow] Failed creating sample grabber");
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
            graphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, sourceObject1, typeof(IAMCrossbar).GUID,
                out retInterface);
            if (retInterface != null)
                crossbar = (IAMCrossbar)retInterface;
            isCrossbarAvailable = crossbar != null;
            crossbarVideoInputs = ColletCrossbarVideoInputs(crossbar);
            sampleGrabber2.SetBufferSamples(false);
            sampleGrabber2.SetOneShot(false);
            sampleGrabber2.SetCallback(callback, 1);
            GetPinCapabilitiesAndConfigureSizeAndRate(graphBuilder, sourceObject1, PinCategory.Capture, VideoResolution,
                ref videoCapabilities);
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
                    callback.Size = new Size(structure.BmiHeader.Width, structure.BmiHeader.Height);
                    mediaType.Dispose();
                }

                var mediaControl2 = (IMediaControl)o2;
                var mediaEventEx2 = (IMediaEventEx)o2;
                mediaControl2.Run();
                StartEvent.Set();
                LogHelper.Instance.Log("[DirectShow] graph is running; avg rate = {0}.",
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
                            LogHelper.Instance.Log("[DirectShow]Event: {0}", lEventCode.ToString());
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

                    if (Triggered)
                    {
                        lock (m_listLock)
                        {
                            ReceivedFrames.Clear();
                        }

                        callback.Grab = true;
                        Thread.Sleep(GrabWaitTime);
                        Triggered = false;
                        callback.Grab = false;
                        if (Debug)
                            LogHelper.Instance.Log("There are {0} frames in the list.", ReceivedFrames.Count);
                        lock (m_listLock)
                        {
                            if (ReceivedFrames.Count > 0 && SnapshotFrame != null)
                            {
                                SnapshotFrame(this, new NewFrameEventArgs(ReceivedFrames[ReceivedFrames.Count - 1]));
                            }
                            else
                            {
                                LogHelper.Instance.Log("[DirectShow] There are no captured frames from the device.");
                                if (mediaEventEx2 != null &&
                                    mediaEventEx2.GetEvent(out lEventCode, out lParam1, out lParam2, 0) == 0U)
                                {
                                    mediaEventEx2.FreeEventParams(lEventCode, lParam1, lParam2);
                                    LogHelper.Instance.Log(">> [DirectShow]Event: {0}", lEventCode.ToString());
                                }
                            }

                            foreach (System.Drawing.Image receivedFrame in ReceivedFrames)
                                receivedFrame.Dispose();
                            ReceivedFrames.Clear();
                        }
                    }
                } while (!stopEvent.WaitOne(100, false));

                mediaControl2.Stop();
                if (Debug)
                    LogHelper.Instance.Log("there were {0} frames received via video.", framesReceived);
            }
        }
        catch (Exception ex)
        {
            if (VideoSourceError != null)
                VideoSourceError(this, new VideoSourceErrorEventArgs(ex.Message));
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
            if (SourceObject != null)
            {
                Marshal.ReleaseComObject(SourceObject);
                SourceObject = null;
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
            if (crossbar != null && crossbar.get_PinCounts(out var _, out inputPinCount) == 0)
                for (var index = 0; index < inputPinCount; ++index)
                {
                    PhysicalConnectorType physicalType;
                    if (crossbar.get_CrossbarPinInfo(true, index, out var _, out physicalType) == 0 &&
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
        if (crossbar.get_PinCounts(out outputPinCount, out var _) == 0)
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

    private void OnNewFrame(Bitmap image)
    {
        ++framesReceived;
        if (!Monitor.TryEnter(m_listLock))
            return;
        try
        {
            if (stopEvent.WaitOne(0, false) || !Triggered)
                return;
            ReceivedFrames.Add((Bitmap)image.Clone());
        }
        finally
        {
            Monitor.Exit(m_listLock);
        }
    }

    private void OnSnapshotFrame(Bitmap image)
    {
        if (SnapshotFrame == null || stopEvent.WaitOne(0, false))
            return;
        SnapshotFrame(this, new NewFrameEventArgs(image));
    }
}
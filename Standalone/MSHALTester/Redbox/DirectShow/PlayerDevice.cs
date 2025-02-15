using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public class PlayerDevice : IVideoSource
{
    private static readonly Dictionary<string, VideoCapabilities[]> cacheVideoCapabilities = new();
    private static readonly Dictionary<string, VideoCapabilities[]> cacheSnapshotCapabilities = new();
    private static readonly Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs = new();
    private readonly bool Debug;
    private readonly int GrabTime;
    private readonly object m_listLock = new();
    private readonly List<Bitmap> ReceivedFrames = new();
    private readonly object sync = new();
    private VideoInput crossbarVideoInput = VideoInput.Default;
    private VideoInput[] crossbarVideoInputs;
    private string deviceMoniker;
    private int framesReceived;
    private bool? isCrossbarAvailable;
    private bool m_simulateTrigger;
    private bool needToDisplayCrossBarPropertyPage;
    private bool needToDisplayPropertyPage;
    private bool needToSetVideoInput;
    private IntPtr parentWindowForPropertyPage = IntPtr.Zero;
    private VideoCapabilities[] snapshotCapabilities;
    private ManualResetEvent stopEvent;
    private Thread thread;
    private VideoCapabilities[] videoCapabilities;

    public PlayerDevice(string deviceMoniker, int grabTime, bool debug)
    {
        this.deviceMoniker = deviceMoniker;
        GrabTime = grabTime;
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

    public VideoCapabilities[] SnapshotCapabilities
    {
        get
        {
            if (snapshotCapabilities == null)
            {
                lock (cacheSnapshotCapabilities)
                {
                    if (!string.IsNullOrEmpty(deviceMoniker))
                        if (cacheSnapshotCapabilities.ContainsKey(deviceMoniker))
                            snapshotCapabilities = cacheSnapshotCapabilities[deviceMoniker];
                }

                if (snapshotCapabilities == null)
                {
                    if (!IsRunning)
                        WorkerThread(false);
                    else
                        for (var index = 0; index < 500 && snapshotCapabilities == null; ++index)
                            Thread.Sleep(10);
                }
            }

            return snapshotCapabilities == null ? new VideoCapabilities[0] : snapshotCapabilities;
        }
    }

    public object SourceObject { get; private set; }

    public event NewFrameEventHandler NewFrame;

    public event VideoSourceErrorEventHandler VideoSourceError;

    public event PlayingFinishedEventHandler PlayingFinished;

    public virtual string Source
    {
        get => deviceMoniker;
        set
        {
            deviceMoniker = value;
            videoCapabilities = null;
            snapshotCapabilities = null;
            crossbarVideoInputs = null;
            isCrossbarAvailable = new bool?();
        }
    }

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
        if (!IsRunning)
        {
            if (string.IsNullOrEmpty(deviceMoniker))
            {
                LogHelper.Instance.Log("[DirectShow] Video source is not specified.");
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
        }

        return true;
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

    public void DisplayPropertyPage(IntPtr parentWindow)
    {
        if (deviceMoniker == null || deviceMoniker == string.Empty)
            throw new ArgumentException("Video source is not specified.");
        lock (sync)
        {
            if (IsRunning)
            {
                parentWindowForPropertyPage = parentWindow;
                needToDisplayPropertyPage = true;
            }
            else
            {
                object filter;
                try
                {
                    filter = FilterInfo.CreateFilter(deviceMoniker);
                }
                catch
                {
                    throw new ApplicationException("Failed creating device object for moniker.");
                }

                if (!(filter is ISpecifyPropertyPages))
                    throw new NotSupportedException("The video source does not support configuration property page.");
                DisplayPropertyPage(parentWindow, filter);
                Marshal.ReleaseComObject(filter);
            }
        }
    }

    public void DisplayCrossbarPropertyPage(IntPtr parentWindow)
    {
        lock (sync)
        {
            for (var index = 0; index < 500 && !isCrossbarAvailable.HasValue && IsRunning; ++index)
                Thread.Sleep(10);
            if (!IsRunning || !isCrossbarAvailable.HasValue)
                throw new ApplicationException(
                    "The video source must be running in order to display crossbar property page.");
            if (!isCrossbarAvailable.Value)
                throw new NotSupportedException(
                    "Crossbar configuration is not supported by currently running video source.");
            parentWindowForPropertyPage = parentWindow;
            needToDisplayCrossBarPropertyPage = true;
        }
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
        m_simulateTrigger = true;
    }

    public bool SetCameraProperty(
        CameraControlProperty property,
        int value,
        CameraControlFlags controlFlags)
    {
        var flag = true;
        if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
            throw new ArgumentException("Video source is not specified.");
        lock (sync)
        {
            object filter;
            try
            {
                filter = FilterInfo.CreateFilter(deviceMoniker);
            }
            catch
            {
                throw new ApplicationException("Failed creating device object for moniker.");
            }

            if (!(filter is IAMCameraControl))
                throw new NotSupportedException("The video source does not support camera control.");
            flag = ((IAMCameraControl)filter).Set(property, value, controlFlags) >= 0;
            Marshal.ReleaseComObject(filter);
        }

        return flag;
    }

    public bool GetCameraProperty(
        CameraControlProperty property,
        out int value,
        out CameraControlFlags controlFlags)
    {
        var cameraProperty = true;
        if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
            throw new ArgumentException("Video source is not specified.");
        lock (sync)
        {
            object filter;
            try
            {
                filter = FilterInfo.CreateFilter(deviceMoniker);
            }
            catch
            {
                throw new ApplicationException("Failed creating device object for moniker.");
            }

            if (!(filter is IAMCameraControl))
                throw new NotSupportedException("The video source does not support camera control.");
            cameraProperty = ((IAMCameraControl)filter).Get(property, out value, out controlFlags) >= 0;
            Marshal.ReleaseComObject(filter);
        }

        return cameraProperty;
    }

    public bool GetCameraPropertyRange(
        CameraControlProperty property,
        out int minValue,
        out int maxValue,
        out int stepSize,
        out int defaultValue,
        out CameraControlFlags controlFlags)
    {
        var cameraPropertyRange = true;
        if (deviceMoniker == null || string.IsNullOrEmpty(deviceMoniker))
            throw new ArgumentException("Video source is not specified.");
        lock (sync)
        {
            object filter;
            try
            {
                filter = FilterInfo.CreateFilter(deviceMoniker);
            }
            catch
            {
                throw new ApplicationException("Failed creating device object for moniker.");
            }

            if (!(filter is IAMCameraControl))
                throw new NotSupportedException("The video source does not support camera control.");
            cameraPropertyRange = ((IAMCameraControl)filter).GetRange(property, out minValue, out maxValue,
                out stepSize, out defaultValue, out controlFlags) >= 0;
            Marshal.ReleaseComObject(filter);
        }

        return cameraPropertyRange;
    }

    private void WorkerThread()
    {
        WorkerThread(true);
    }

    private void WorkerThread(bool runGraph)
    {
        var reason = ReasonToFinishPlaying.StoppedByUser;
        var callback = new SampleGrabber(OnNewFrame);
        callback.Grab = true;
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
        var amCrossbar = (IAMCrossbar)null;
        try
        {
            o1 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2) ??
                                          throw new ApplicationException("Failed creating capture graph builder"));
            var graphBuilder = (ICaptureGraphBuilder2)o1;
            o2 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph) ??
                                          throw new ApplicationException("Failed creating filter graph"));
            var filterGraph2_2 = (IFilterGraph2)o2;
            graphBuilder.SetFiltergraph((IGraphBuilder)filterGraph2_2);
            this.SourceObject = FilterInfo.CreateFilter(deviceMoniker);
            var baseFilter3 = this.SourceObject != null
                ? (IBaseFilter)this.SourceObject
                : throw new ApplicationException("Failed creating device object for moniker");
            try
            {
                var sourceObject = (IAMVideoControl)this.SourceObject;
            }
            catch
            {
            }

            o3 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SampleGrabber) ??
                                          throw new ApplicationException("Failed creating sample grabber"));
            var sampleGrabber2 = (ISampleGrabber)o3;
            var baseFilter4 = (IBaseFilter)o3;
            filterGraph2_2.AddFilter(baseFilter3, "source");
            filterGraph2_2.AddFilter(baseFilter4, "grabber_video");
            var mediaType = new AMMediaType();
            mediaType.MajorType = MediaType.Video;
            mediaType.SubType = MediaSubType.RGB24;
            sampleGrabber2.SetMediaType(mediaType);
            graphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, baseFilter3, typeof(IAMCrossbar).GUID,
                out retInterface);
            if (retInterface != null)
                amCrossbar = (IAMCrossbar)retInterface;
            isCrossbarAvailable = amCrossbar != null;
            crossbarVideoInputs = ColletCrossbarVideoInputs(amCrossbar);
            sampleGrabber2.SetBufferSamples(false);
            sampleGrabber2.SetOneShot(false);
            sampleGrabber2.SetCallback(callback, 1);
            GetPinCapabilitiesAndConfigureSizeAndRate(graphBuilder, baseFilter3, PinCategory.Capture, VideoResolution,
                ref videoCapabilities);
            snapshotCapabilities = new VideoCapabilities[0];
            lock (cacheVideoCapabilities)
            {
                if (videoCapabilities != null)
                    if (!cacheVideoCapabilities.ContainsKey(deviceMoniker))
                        cacheVideoCapabilities.Add(deviceMoniker, videoCapabilities);
            }

            lock (cacheSnapshotCapabilities)
            {
                if (snapshotCapabilities != null)
                    if (!cacheSnapshotCapabilities.ContainsKey(deviceMoniker))
                        cacheSnapshotCapabilities.Add(deviceMoniker, snapshotCapabilities);
            }

            if (runGraph)
            {
                graphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, baseFilter3, null, baseFilter4);
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
                do
                {
                    DsEvCode lEventCode;
                    IntPtr lParam1;
                    IntPtr lParam2;
                    if (mediaEventEx2 != null &&
                        mediaEventEx2.GetEvent(out lEventCode, out lParam1, out lParam2, 0) >= 0U)
                    {
                        mediaEventEx2.FreeEventParams(lEventCode, lParam1, lParam2);
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
                            SetCurrentCrossbarInput(amCrossbar, crossbarVideoInput);
                            crossbarVideoInput = GetCurrentCrossbarInput(amCrossbar);
                        }
                    }

                    if (m_simulateTrigger)
                    {
                        m_simulateTrigger = false;
                        lock (m_listLock)
                        {
                            ReceivedFrames.Clear();
                        }

                        callback.Grab = true;
                        Thread.Sleep(GrabTime);
                        m_simulateTrigger = callback.Grab = false;
                        if (Debug)
                            LogHelper.Instance.Log("There are {0} frames in the list.", ReceivedFrames.Count);
                        lock (m_listLock)
                        {
                            if (ReceivedFrames.Count > 0 && SnapshotFrame != null)
                                SnapshotFrame(this, new NewFrameEventArgs(ReceivedFrames[ReceivedFrames.Count - 1]));
                            else
                                LogHelper.Instance.Log("[DirectShow] There are no captured frames from the device.");
                            foreach (System.Drawing.Image receivedFrame in ReceivedFrames)
                                receivedFrame.Dispose();
                            ReceivedFrames.Clear();
                        }
                    }

                    if (needToDisplayPropertyPage)
                    {
                        needToDisplayPropertyPage = false;
                        DisplayPropertyPage(parentWindowForPropertyPage, SourceObject);
                        if (amCrossbar != null)
                            crossbarVideoInput = GetCurrentCrossbarInput(amCrossbar);
                    }

                    if (needToDisplayCrossBarPropertyPage)
                    {
                        needToDisplayCrossBarPropertyPage = false;
                        if (amCrossbar != null)
                        {
                            DisplayPropertyPage(parentWindowForPropertyPage, amCrossbar);
                            crossbarVideoInput = GetCurrentCrossbarInput(amCrossbar);
                        }
                    }
                } while (!stopEvent.WaitOne(100, false));

                mediaControl2.Stop();
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

    private void DisplayPropertyPage(IntPtr parentWindow, object sourceObject)
    {
        try
        {
            CAUUID pPages;
            ((ISpecifyPropertyPages)sourceObject).GetPages(out pPages);
            var filterInfo = new FilterInfo(deviceMoniker);
            Win32.OleCreatePropertyFrame(parentWindow, 0, 0, filterInfo.Name, 1, ref sourceObject, pPages.cElems,
                pPages.pElems, 0, 0, IntPtr.Zero);
            Marshal.FreeCoTaskMem(pPages.pElems);
        }
        catch
        {
        }
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
        if (stopEvent.WaitOne(0, false) || NewFrame == null)
            return;
        NewFrame(this, new NewFrameEventArgs(image));
    }
}
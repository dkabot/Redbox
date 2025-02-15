using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Redbox.DirectShow.Interop;

namespace Redbox.DirectShow
{
    public class VideoCapabilities
    {
        public readonly int AverageFrameRate;
        public readonly int BitCount;
        public readonly Size FrameSize;
        public readonly int MaximumFrameRate;

        internal VideoCapabilities()
        {
        }

        internal VideoCapabilities(IAMStreamConfig videoStreamConfig, int index)
        {
            var mediaType = (AMMediaType)null;
            var streamConfigCaps = new VideoStreamConfigCaps();
            try
            {
                var streamCaps = videoStreamConfig.GetStreamCaps(index, out mediaType, streamConfigCaps);
                if (streamCaps != 0)
                    Marshal.ThrowExceptionForHR(streamCaps);
                if (mediaType.FormatType == FormatType.VideoInfo)
                {
                    var structure =
                        (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                    FrameSize = new Size(structure.BmiHeader.Width, structure.BmiHeader.Height);
                    BitCount = structure.BmiHeader.BitCount;
                    AverageFrameRate = (int)(10000000L / structure.AverageTimePerFrame);
                    MaximumFrameRate = (int)(10000000L / streamConfigCaps.MinFrameInterval);
                }
                else
                {
                    if (!(mediaType.FormatType == FormatType.VideoInfo2))
                        throw new ApplicationException("Unsupported format found.");
                    var structure =
                        (VideoInfoHeader2)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader2));
                    FrameSize = new Size(structure.BmiHeader.Width, structure.BmiHeader.Height);
                    BitCount = structure.BmiHeader.BitCount;
                    AverageFrameRate = (int)(10000000L / structure.AverageTimePerFrame);
                    MaximumFrameRate = (int)(10000000L / streamConfigCaps.MinFrameInterval);
                }

                if (BitCount <= 12)
                    throw new ApplicationException("Unsupported format found.");
            }
            finally
            {
                mediaType?.Dispose();
            }
        }

        [Obsolete("No longer supported. Use AverageFrameRate instead.")]
        public int FrameRate => AverageFrameRate;

        public static bool operator ==(VideoCapabilities a, VideoCapabilities b)
        {
            if (a == (object)b)
                return true;
            return (object)a != null && (object)b != null && a.Equals(b);
        }

        public static bool operator !=(VideoCapabilities a, VideoCapabilities b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as VideoCapabilities);
        }

        public bool Equals(VideoCapabilities vc2)
        {
            return (object)vc2 != null && FrameSize == vc2.FrameSize && BitCount == vc2.BitCount;
        }

        public override int GetHashCode()
        {
            return this.FrameSize.GetHashCode() ^ this.BitCount;
        }

        internal static VideoCapabilities[] FromStreamConfig(IAMStreamConfig videoStreamConfig)
        {
            int count;
            int size;
            var errorCode = videoStreamConfig != null
                ? videoStreamConfig.GetNumberOfCapabilities(out count, out size)
                : throw new ArgumentNullException(nameof(videoStreamConfig));
            if (errorCode != 0)
                Marshal.ThrowExceptionForHR(errorCode);
            if (count <= 0)
                throw new NotSupportedException("This video device does not report capabilities.");
            if (size > Marshal.SizeOf(typeof(VideoStreamConfigCaps)))
                throw new NotSupportedException(
                    "Unable to retrieve video device capabilities. This video device requires a larger VideoStreamConfigCaps structure.");
            var dictionary = new Dictionary<uint, VideoCapabilities>();
            for (var index = 0; index < count; ++index)
                try
                {
                    var videoCapabilities = new VideoCapabilities(videoStreamConfig, index);
                    var key = (uint)(videoCapabilities.FrameSize.Height | (videoCapabilities.FrameSize.Width << 16));
                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, videoCapabilities);
                    else if (videoCapabilities.BitCount > dictionary[key].BitCount)
                        dictionary[key] = videoCapabilities;
                }
                catch
                {
                }

            var array = new VideoCapabilities[dictionary.Count];
            dictionary.Values.CopyTo(array, 0);
            return array;
        }
    }
}
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class ObjectExtensions
    {
        public static StringContent ToStandardPayloadContent(this object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj, 0, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }), Encoding.UTF8, "application/json");
        }

        public static HttpContent ToGZipPayloadContent(this object source)
        {
            return new CompressedContent(source.ToStandardPayloadContent(), "gzip");
        }

        public static HttpContent ToDeflatePayloadContent(this object source)
        {
            return new CompressedContent(source.ToStandardPayloadContent(), "deflate");
        }

        private class CompressedContent : HttpContent
        {
            private readonly string encodingType;

            private readonly HttpContent originalContent;

            public CompressedContent(HttpContent content, string encodingType)
            {
                if (content == null) throw new ArgumentNullException("content");
                if (encodingType == null) throw new ArgumentNullException("encodingType");
                originalContent = content;
                this.encodingType = encodingType.ToLowerInvariant();
                if (this.encodingType != "gzip" && this.encodingType != "deflate")
                    throw new InvalidOperationException(string.Format(
                        "Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", this.encodingType));
                foreach (var keyValuePair in originalContent.Headers)
                    Headers.TryAddWithoutValidation(keyValuePair.Key, keyValuePair.Value);
                Headers.ContentEncoding.Add(encodingType);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = -1L;
                return false;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                Stream compressedStream = null;
                if (encodingType == "gzip")
                    compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
                else if (encodingType == "deflate")
                    compressedStream = new DeflateStream(stream, CompressionMode.Compress, true);
                return originalContent.CopyToAsync(compressedStream).ContinueWith(delegate
                {
                    if (compressedStream != null) compressedStream.Dispose();
                });
            }
        }
    }
}
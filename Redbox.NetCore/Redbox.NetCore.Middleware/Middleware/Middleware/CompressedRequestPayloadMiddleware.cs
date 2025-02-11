using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Redbox.NetCore.Middleware.Middleware
{
    public class CompressedRequestPayloadMiddleware
    {
        private readonly RequestDelegate next;

        public CompressedRequestPayloadMiddleware(RequestDelegate next)
        {
            if (next == null) throw new ArgumentNullException("next");
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains("Content-Encoding"))
            {
                var stringValues = context.Request.Headers["Content-Encoding"];
                if (Headers.ContentEncodingValues.Contains(stringValues))
                {
                    string text = stringValues;
                    if (!(text == "gzip"))
                    {
                        if (text == "deflate")
                            context.Request.Body =
                                new DeflateStream(context.Request.Body, CompressionMode.Decompress, true);
                    }
                    else
                    {
                        context.Request.Body = new GZipStream(context.Request.Body, CompressionMode.Decompress, true);
                    }
                }
            }

            await next(context);
        }

        private static class Headers
        {
            public const string ContentEncoding = "Content-Encoding";

            public static class ContentEncodingValues
            {
                public const string GZip = "gzip";

                public const string Deflate = "deflate";

                public static bool Contains(string value)
                {
                    return value == "gzip" || value == "deflate";
                }
            }
        }
    }
}
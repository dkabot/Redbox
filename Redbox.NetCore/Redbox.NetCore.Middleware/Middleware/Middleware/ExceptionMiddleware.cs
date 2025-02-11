using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace Redbox.NetCore.Middleware.Middleware
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IGrafanaMetrics _metrics;
        private Stream _responseStream;

        public ExceptionMiddleware(
            IHostingEnvironment env,
            ILogger<ExceptionMiddleware> logger,
            IGrafanaMetrics metrics)
        {
            _env = env;
            _logger = logger;
            _metrics = metrics;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _responseStream = context.Response.Body;
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                if (!context.Response.HasStarted)
                {
                    await HandleExceptionAsync(context, ex);
                }
                else
                {
                    _logger.LogError(ex,
                        "Exception: " + ex.GetType().Name + " RequestUrl: " + context.Request.GetDisplayUrl());
                    _metrics.CaptureError(ex);
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception error)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            var errorId = Guid.NewGuid();
            _metrics.CaptureError(error);
            var requestBody = await context.Request.ReadBodyAsync();
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, errorId,
                string.Format("Error Reference {0}", errorId), error.GetFullMessage());
            _logger.LogError(error,
                string.Format("Reference: {0} Exception: {1} RequestUrl: {2} RequestBody: {3}", errorId,
                    error.GetType().Name, context.Request.GetDisplayUrl(), requestBody));
            requestBody = null;
        }

        private async Task WriteErrorAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            Guid errorId,
            string prodMessage,
            string devMessage = "")
        {
            context.Response.Body = _responseStream;
            context.Response.StatusCode = (int)statusCode;
            var activityId = context.GetActivityId();
            var kioskId = context.GetKioskId();
            var str = prodMessage;
            if (_env.IsDevelopment() && !string.IsNullOrWhiteSpace(devMessage))
                str = devMessage;
            if (_responseStream == null || !_responseStream.CanWrite)
                return;
            if (context.Response.HasStarted)
                return;
            try
            {
                context.Response.Body = _responseStream;
                context.Response.StatusCode = (int)statusCode;
                await context.Response.WriteAsync(new Error
                {
                    Code = statusCode.ToString(),
                    Message = str,
                    Id = errorId,
                    ActivityId = activityId,
                    KioskId = kioskId
                }.ToJson());
            }
            catch (ObjectDisposedException ex)
            {
            }
        }
    }
}
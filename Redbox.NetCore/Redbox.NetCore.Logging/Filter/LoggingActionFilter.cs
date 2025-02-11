using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Abstractions;

namespace Redbox.NetCore.Logging.Filter
{
    internal class LoggingActionFilter : IAsyncActionFilter, IFilterMetadata
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoggingActionFilter> _logger;

        public LoggingActionFilter(ILogger<LoggingActionFilter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            var controllerName = actionDescriptor.ControllerName;
            var actionName = actionDescriptor.ActionName;
            var scrubbedRequest = new StringBuilder();
            context.ActionArguments.ToList().ForEach(arg =>
            {
                if (arg.Value is IMessageScrub)
                    scrubbedRequest.Append(string.Format(",{0}:{1}", arg.Key, (arg.Value as IMessageScrub).Scrub()));
                else
                    scrubbedRequest.Append(string.Format(",{0}:{1}", arg.Key, arg.Value));
            });
            _logger.LogInformation(string.Format("START: {0}.{1} {2}", controllerName, actionName, scrubbedRequest));
            var requestTimer = Stopwatch.StartNew();
            var actionExecutedContext = await next.Invoke();
            var stringBuilder = new StringBuilder();
            if (actionExecutedContext?.Result != null)
            {
                if (actionExecutedContext.Result is OkObjectResult result3)
                {
                    if (result3.Value is IMessageScrub messageScrub1)
                        stringBuilder.Append(string.Format("{0}", messageScrub1.Scrub()));
                    else
                        stringBuilder.Append(string.Format("{0}", result3.StatusCode));
                }
                else if (actionExecutedContext.Result is ContentResult result2)
                {
                    stringBuilder.Append(result2.Content);
                }
                else if (actionExecutedContext.Result is ObjectResult result1)
                {
                    if (result1.Value is IMessageScrub messageScrub2)
                        stringBuilder.Append(string.Format("{0}", messageScrub2.Scrub()));
                    else
                        stringBuilder.Append(string.Format("{0}", result1.StatusCode));
                }

                var nullable = (int?)actionExecutedContext.Result.GetType().GetProperty("StatusCode")
                    ?.GetValue(actionExecutedContext.Result);
                if (nullable.HasValue)
                    context.HttpContext.Response.StatusCode = nullable.Value;
            }
            else
            {
                stringBuilder.Append("none");
            }

            requestTimer.Stop();
            _logger.LogInformation(string.Format("{0}.{1},response={2}", controllerName, actionName, stringBuilder));
            _logger.LogInformation(string.Format("END: {0}.{1},statuscode={2},elapsed={3}", controllerName, actionName,
                (HttpStatusCode)context.HttpContext.Response.StatusCode, requestTimer.ElapsedMilliseconds));
            controllerName = null;
            actionName = null;
            requestTimer = null;
        }
    }
}
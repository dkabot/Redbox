using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

namespace Redbox.NetCore.Middleware.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DisableRequestBodyDataRateAttribute : Attribute, IResourceFilter, IFilterMetadata
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var bodyDataRateFeature = context.HttpContext.Features.Get<IHttpMinRequestBodyDataRateFeature>();
            if (bodyDataRateFeature == null)
                return;
            bodyDataRateFeature.MinDataRate = null;
        }
    }
}
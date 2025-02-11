using Microsoft.AspNetCore.Builder;
using Redbox.NetCore.Middleware.Filter;
using Redbox.NetCore.Middleware.Middleware;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseActivityIdMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ActivityIdMiddleware>();
        }

        public static IApplicationBuilder UseMetricsActionFilterExtensions(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricsActionFilter>();
        }

        public static IApplicationBuilder UseKioskIdMiddleWare(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<KioskIdMiddleware>();
        }

        public static IApplicationBuilder UseSessionIdMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionIdMiddleware>();
        }

        public static IApplicationBuilder UseGrafanaMetricsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GrafanaMetricsMiddleware>().UseMiddleware<ExceptionMiddleware>();
        }

        public static void AddCompressedRequestPayloadMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CompressedRequestPayloadMiddleware>();
        }
    }
}
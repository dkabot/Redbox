using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DeviceService.Client.Core;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Analytics;
using DeviceService.ComponentModel.Bluefin;
using DeviceService.ComponentModel.FileUpdate;
using DeviceService.ComponentModel.KDS;
using DeviceService.Domain;
using DeviceService.Domain.FileUpdate;
using DeviceService.WebApi.Bluefin;
using DeviceService.WebApi.KDS;
using DeviceService.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RBA_SDK_ComponentModel;
using RBA_SDK;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace DeviceService.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion((CompatibilityVersion)1);
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info
            {
                Title = "My API",
                Version = "v1"
            }));
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<DeviceServiceConfig>(Configuration.GetSection("DeviceServiceConfig"));
            var provider1 = services.BuildServiceProvider();
            var implementationInstance = provider1.GetRequiredService<IOptionsSnapshot<ApplicationSettings>>().Value;
            services.AddSingleton((IApplicationSettings)implementationInstance);
            var requiredService = provider1.GetRequiredService<ILogger<Startup>>();
            requiredService.LogInformation(string.Format("AppSettings - UseSimulator: {0}",
                implementationInstance.UseSimulator));
            requiredService.LogInformation("AppSettings - DataFilePath: " + implementationInstance.DataFilePath);
            requiredService.LogInformation("AppSettings - DeviceServiceUrl: " +
                                           implementationInstance.DeviceServiceUrl);
            requiredService.LogInformation("AppSettings - DeviceServiceClientPath: " +
                                           implementationInstance.DeviceServiceClientPath);
            if (implementationInstance.UseSimulator)
            {
                if (File.Exists(".\\RBA_SDK_Simulator.dll"))
                {
                    var assembly = Assembly.LoadFrom(".\\RBA_SDK_Simulator.dll");
                    if (assembly != null)
                    {
                        Log.Information("Using RBA_SDK_Simulator");
                        services.AddMvc().AddApplicationPart(assembly).AddControllersAsServices();
                        var type1 = assembly.GetType("RBA_SDK_Simulator.ISimulatorDataProvider");
                        var type2 = assembly.GetType("RBA_SDK_Simulator.SimulatorDataProvider");
                        var type3 = assembly.GetType("RBA_SDK_Simulator.ICommandQueue");
                        var type4 = assembly.GetType("RBA_SDK_Simulator.CommandQueue");
                        services.AddSingleton(type1, type2);
                        services.AddSingleton(type3, type4);
                        services.AddSingleton(typeof(IRBA_API),
                            assembly.GetType("RBA_SDK_Simulator.RBA_API_Simulator"));
                    }
                }
            }
            else
            {
                services.AddSingleton<IRBA_API, RBA_API>();
            }

            services.AddSingleton((Func<IServiceProvider, IAnalyticsService>)(provider =>
                new AnalyticsService(provider.GetRequiredService<IApplicationSettings>(),
                    provider.GetRequiredService<ILogger<AnalyticsService>>())));
            services.AddSingleton((Func<IServiceProvider, IIUC285Notifier>)(provider =>
                new IUC285Notifier(provider.GetRequiredService<ILogger<IUC285Notifier>>(),
                    provider.GetRequiredService<IHttpService>(), provider.GetRequiredService<IApplicationSettings>())));
            var singletonIUC285Proxy = (IIUC285Proxy)null;
            services.AddSingleton((Func<IServiceProvider, IIUC285Proxy>)(provider =>
            {
                if (singletonIUC285Proxy == null)
                {
                    singletonIUC285Proxy = new IUC285Proxy(provider.GetRequiredService<IRBA_API>(),
                        provider.GetRequiredService<ILogger<IUC285Proxy>>(),
                        provider.GetRequiredService<IIUC285Notifier>(),
                        provider.GetRequiredService<IKioskDataServiceClient>(),
                        provider.GetRequiredService<IApplicationSettings>(),
                        provider.GetRequiredService<IOptionsMonitor<DeviceServiceConfig>>());
                    Log.Information("Begin Connect");
                    if (singletonIUC285Proxy is IUC285Proxy iuC285Proxy2) iuC285Proxy2.Connect();
                }

                return singletonIUC285Proxy;
            }));
            services.AddSingleton(
                (Func<IServiceProvider, IFileUpdater>)(provider => singletonIUC285Proxy as IFileUpdater));
            services.AddSingleton(Log.Logger);
            services.AddSignalR();
            services.AddSingleton<IBluefinServiceClient, BluefinServiceClient>();
            services.AddSingleton<IKioskDataServiceClient, KioskDataServiceClient>();
            services.AddSingleton<IDeviceServiceClientCore, DeviceServiceClient>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IDeviceStatusService, DeviceStatusService>();
            services.AddSingleton<IHttpService, HttpService>();
            var singletonApplicationControl = (IApplicationControl)null;
            services.AddSingleton((Func<IServiceProvider, IApplicationControl>)(provider =>
            {
                if (singletonApplicationControl == null)
                    singletonApplicationControl = new ApplicationControl(
                        provider.GetRequiredService<IApplicationLifetime>(),
                        provider.GetRequiredService<ILogger<ApplicationControl>>(),
                        provider.GetRequiredService<IIUC285Notifier>());
                return singletonApplicationControl;
            }));
            services.AddSingleton<IFileUpdateService, FileUpdateService>();
            new Task(() =>
            {
                var provider2 = services.BuildServiceProvider();
                provider2.GetRequiredService<IDeviceStatusService>().PostDeviceStatus();
                provider2.GetRequiredService<IFileUpdateService>();
            }).Start();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
            app.UseMvc();
            app.UseSignalR(route => route.MapHub<CardReaderHub>("/CardReaderEvents"));
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DeviceService.WebApi
{
    public class Program
    {
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

        public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

        public static void Main(string[] args)
        {
            var flag = !Debugger.IsAttached && !args.Contains("--console");
            if (flag)
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            ApplicationControl.IsService = flag;
            var configuration = Configuration;
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).Enrich.WithThreadId().Enrich
                .FromLogContext().WriteTo.Console().CreateLogger();
            try
            {
                Log.Information(
                    "------------------------------------ Starting web host -------------------------------------");
                Log.Information("ASPNETCORE_ENVIRONMENT: " +
                                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                Log.Information(string.Format("DeviceService version: {0}", Domain.DeviceService.AssemblyVersion));
                if (!flag)
                {
                    Log.Information("Program.Main - Starting as a console application");
                    CreateWebHostBuilder(args, configuration).Build().Run();
                }
                else
                {
                    Log.Information("Program.Main - Starting as a Windows Service");
                    CreateWebHostBuilder(args, configuration).Build().RunAsDeviceServiceService();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program.Main - Web host terminated with error");
            }
            finally
            {
                Log.Information(
                    "------------------------------------ Stopping web host -------------------------------------");
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration config)
        {
            var deviceServiceUri = GetDeviceServiceUri(config);
            Log.Information(string.Format("CreateWebHostBuilder: url={0}  port={1}", deviceServiceUri.AbsoluteUri,
                deviceServiceUri.Port));
            return WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
                .UseUrls(deviceServiceUri.AbsoluteUri).UseSetting("Port", deviceServiceUri.Port.ToString())
                .ConfigureAppConfiguration((builder, conf) => GetConfiguration(conf)).UseSerilog()
                .UseStartup<Startup>();
        }

        public static Uri GetDeviceServiceUri(IConfiguration config)
        {
            return new Uri(config.GetValue<string>("AppSettings:DeviceServiceUrl"));
        }

        private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("Data\\DeviceServiceConfig.json", true, true);
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null)
                builder.AddJsonFile(
                    "appsettings." + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + ".json", true,
                    true);
            return builder;
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Redbox.NetCore.Middleware.Extensions;
using Serilog;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API
{
    public class Program
    {
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
        public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

        public static void Main(string[] args)
        {
            var flag = !Debugger.IsAttached && !args.Contains("--console");
            if (flag)
            {
                var fileName = Process.GetCurrentProcess().MainModule.FileName;
                var directoryName = Path.GetDirectoryName(fileName);
                Directory.SetCurrentDirectory(directoryName);
            }

            try
            {
                var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).Enrich.WithThreadId()
                    .Enrich.FromLogContext().WriteTo.Console().CreateLogger();
                Log.Logger = logger;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered unhandled exception while configuring logger. Exception -> {0}", ex);
                return;
            }

            try
            {
                Log.Information(
                    "------------------------------------ Starting web host -------------------------------------");
                Log.Information("ASPNETCORE_ENVIRONMENT: " +
                                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                Log.Information(
                    string.Format("Service version: {0}", Assembly.GetExecutingAssembly().GetName().Version));
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (!flag)
                {
                    Log.Information("Program.Main - Starting as a console application");
                    CreateWebHostBuilder(args).Build().Run();
                }
                else
                {
                    Log.Information("Program.Main - Starting as a Windows Service");
                    CreateWebHostBuilder(args).Build().RunAsWindowsService();
                }
            }
            catch (Exception ex2)
            {
                Log.Fatal(ex2, "Program.Main - Web host terminated with error");
            }
            finally
            {
                Log.Information(
                    "------------------------------------ Stopping web host -------------------------------------");
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var serviceUri = GetServiceUri(Configuration);
            Log.Information(string.Format("CreateWebHostBuilder: url={0}  port={1}", serviceUri.AbsoluteUri,
                serviceUri.Port));
            return WebHost.CreateDefaultBuilder(args).UseUrls(serviceUri.AbsoluteUri)
                .UseSetting("Port", serviceUri.Port.ToString())
                .UseSerilog()
                .ConfigureAppConfiguration(
                    delegate(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder conf)
                    {
                        GetConfiguration(conf, webHostBuilderContext);
                    })
                .UseStartup<Startup>()
                .UseAppMetrics();
        }

        private static Uri GetServiceUri(IConfiguration config)
        {
            return new Uri(config.GetValue("AppSettings:BaseServiceUrl", string.Empty));
        }

        private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder,
            WebHostBuilderContext webHostBuilderContext = null)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true)
                .AddJsonFile(ConfigurationService.ConfigurationFilePath, true, true);
            string text;
            if (webHostBuilderContext == null)
            {
                text = null;
            }
            else
            {
                var hostingEnvironment = webHostBuilderContext.HostingEnvironment;
                text = hostingEnvironment != null ? hostingEnvironment.EnvironmentName : null;
            }

            var text2 = text ?? "Production";
            builder.AddJsonFile("appsettings." + text2 + ".json", true, true);
            return builder;
        }
    }
}
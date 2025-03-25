using HaasConnectorAPIService.Communication;

namespace HaasConnectorService;

/// <summary>
/// Start of program
/// </summary>
public class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               });

    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        try
        {
            Directory.SetCurrentDirectory(Reflections.GetEntryAssemblyLocation().FullName);

            DirectoryInfo logDirectory = Reflections.GetRootRelativeDir(Directory.GetCurrentDirectory(), "Logs");
            string logfilepath = Path.Combine(logDirectory.FullName, "haas.log");

            //Log.Logger = new LoggerConfiguration().WriteTo.GrafanaLoki("http://localhost:3100", labels: new[]
            //                    {
            //                      new LokiLabel { Key = "HaasConnector", Value = "HaasConnector" }
            //                    }).Enrich.FromLogContext()
            //    .Enrich.WithExceptionDetails()
            //    .WriteTo.Console().CreateLogger();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.File(logfilepath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger.Information("Haas Connector App starting");

            IWebHost host = CreateWebHostBuilder(args).Build();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))//If Windows OS 
            {
                host.RunAsService(); 
            }
            else//All other OS
            {
                host.Run();
            }

            Log.Logger.Information("Haas Connector App started");
        }
        catch (Exception ex)
        {
            Log.Logger.Error($"Application startup failed: {ex.Message}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Host Builder
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                       .SetBasePath(Reflections.GetCurrentAssemblyLocation().FullName)
                       .AddJsonFile("HaasSettings.json", optional: false, reloadOnChange: true)
                .Build();

        IConfigurationSection appSettingsSection = configuration.GetSection("HaasSettings");
        HaasSettings HaasSettings = appSettingsSection.Get<HaasSettings>();

        return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .UseKestrel(options =>
                {
                    options.AddServerHeader = false;

                    options.Limits.MaxRequestBodySize = null;

                    options.Listen(IPAddress.Any, HaasSettings.HttpPort);

                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                }).UseUrls();
    }
}

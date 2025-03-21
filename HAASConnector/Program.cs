namespace HaasConnectorService;

/// <summary>
/// Start of program
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        try
        {
            DirectoryInfo logDirectory = Reflections.GetRootRelativeDir(Directory.GetCurrentDirectory(), "Logs");
            string logfilepath = Path.Combine(logDirectory.FullName, "haas.log");

            Log.Logger = new LoggerConfiguration().WriteTo.GrafanaLoki("http://localhost:3100", labels: new[]
                                {
                                  new LokiLabel { Key = "HaasConnectorService", Value = "HaasConnectorServiceApi" }
                                }).CreateLogger();

            Log.Logger.Information("Haas App starting");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.File(logfilepath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();

            Log.Logger.Information("Haas App started");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application startup failed: {ex.Message}");
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
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
            });
                //Configuration load
                IConfigurationRoot config = new ConfigurationBuilder()
                       .SetBasePath(Reflections.GetCurrentAssemblyLocation().FullName)
                       .AddJsonFile("HaasSettings.json", optional: false, reloadOnChange: true)
                       .Build();
                webBuilder.UseStartup<Startup>();
                webBuilder.UseConfiguration(config);
            }).UseSerilog();
}

namespace HaasConnectorService;

/// <summary>
/// Program Startup
/// </summary>
public class Startup
{
    /// <summary>
    /// Configuration
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// FileScannerSettings
    /// </summary>
    public HaasSettings HaasSettings { get; private set; }

    /// <summary>
    /// Startup
    /// </summary>
    /// <param name="configuration"></param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Configure Services
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        //Configuration
        services.Configure<HaasSettings>(Configuration.GetSection("HaasSettings"));

        //Configuration load
        IConfigurationRoot config = new ConfigurationBuilder()
               .SetBasePath(Reflections.GetCurrentAssemblyLocation().FullName)
               .AddJsonFile("HaasSettings.json", optional: false, reloadOnChange: true)
               .Build();

        IConfigurationSection appSettingsSection = config.GetSection("HaasSettings");
        HaasSettings = appSettingsSection.Get<HaasSettings>();

        //APi Versioning
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });

        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Haas Data API", Version = "v1" }); });

        string[] corsPaths = EnableCors(HaasSettings);

        //cors(cross origin policy)
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                    .WithOrigins(corsPaths)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
                });
        });

        //DI
        services.AddDbContext<AppDbContext>();//SQLIte DB
        //services.AddHostedService<HaasService>();
        services.AddEndpointsApiExplorer();
        services.AddControllersWithViews();
    }

    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("AllowAll");
        app.UseRouting();
        //AddHttpSecurityHeaders(app);

        app.UseSwagger();
        app.UseSwaggerUI(c =>
         {
             c.SwaggerEndpoint("/swagger/v1/swagger.json", "Haas Data API v1");
             c.RoutePrefix = string.Empty; // Set Swagger as the default page
         });

        //Error handling middleware
        app.UseMiddleware(typeof(ErrorHandlingMiddleware));

        app.UseEndpoints(endpoints =>
       {
           endpoints.MapControllers();
       });
    }

    /// <summary>
    /// Enable cors setting
    /// </summary>
    /// <param name="appSettings"></param>
    private string[] EnableCors(HaasSettings appSettings)
    {
        string serverName = Environment.MachineName;
        string fqhn = System.Net.Dns.GetHostEntry(serverName).HostName;

        List<string> corsPaths = new List<string>();

        int HttpPort = appSettings.HttpPort;
        string CorsOrigins = "http://localhost:{{HttpPort}},https://localhost:{{HttpPort}}";

        corsPaths.AddRange(CorsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries));

        if (!string.IsNullOrEmpty(fqhn))
        {
            corsPaths.Add($"http://{fqhn}:{appSettings.HttpPort}");
            corsPaths.Add($"http://0.0.0.0:{appSettings.HttpPort}");

            for (int index = 0; index < corsPaths.Count; index++)
            {
                string content = corsPaths[index];
                content = content.Replace("{{HttpPort}}", appSettings.HttpPort.ToString());
                corsPaths[index] = content;
            }
        }

        return corsPaths.ToArray();
    }

    /// <summary>
    /// Add http security headers using middleware
    /// <param name="app"></param>
    private void AddHttpSecurityHeaders(IApplicationBuilder app)
    {
        StringBuilder contentPolicy = new StringBuilder();
        contentPolicy.Append("default-src 'none';");
        contentPolicy.Append("script-src 'self';");
        contentPolicy.Append("connect-src 'self';");
        contentPolicy.Append("img-src 'self' data:;");
        contentPolicy.Append("frame-ancestors 'none'");

        string ContentPolicy = contentPolicy.ToString();

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                //Security headers
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Content-Security-Policy", ContentPolicy);
                context.Response.Headers.Add("X-FRAME-OPTIONS", "DENY");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "master-only");
                context.Response.Headers.Add("X-Download-Options", "noopen");
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");

                if (!context.Response.Headers.ContainsKey("Cache-Control"))
                    context.Response.Headers.Add("Cache-Control", "no-cache");
                context.Response.Headers.Add("Pragma", "no-cache");

                return Task.FromResult(0);
            });

            await next();
        });
    }
}

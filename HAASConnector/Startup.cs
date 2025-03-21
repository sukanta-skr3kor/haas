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

        //cors(cross origin policy)
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
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
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
         {
             c.SwaggerEndpoint("/swagger/v1/swagger.json", "Haas Data API v1");
             c.RoutePrefix = string.Empty; // Set Swagger as the default page
         });

        app.UseCors("CorsPolicy");
        app.UseHttpsRedirection();
        app.UseRouting();

        //Error handling middleware
        app.UseMiddleware(typeof(ErrorHandlingMiddleware));

        app.UseEndpoints(endpoints =>
       {
           endpoints.MapControllers();
       });
    }
}

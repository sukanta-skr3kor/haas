/// <summary>
/// Defines the <see cref="AppDbContext" />
/// </summary>

public class AppDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the data
    /// Haas Machine Data Store
    /// </summary>
    public DbSet<HaasDB> HaasData { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options<see cref="DbContextOptions{AppDbContext}"/></param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// The OnConfiguring
    /// </summary>
    /// <param name="optionsBuilder">The optionsBuilder<see cref="DbContextOptionsBuilder"/></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)  // Ensure only configure if not already set
        {
            optionsBuilder.UseSqlite("Data Source=Haas.db");
        }
    }
}

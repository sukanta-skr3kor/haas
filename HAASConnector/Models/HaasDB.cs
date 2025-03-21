namespace HaasConnectorService.Models;

/// <summary>
/// SQLite Store
/// </summary>
public class HaasDB
{
    /// <summary>
    /// Gets or sets the Id
    /// </summary>
    [Key]
    public int Id { get; set; }
    public string ParamaterName { get; set; }
    public string Value { get; set; }
    public DateTime time { get; set; } = DateTime.UtcNow;
}

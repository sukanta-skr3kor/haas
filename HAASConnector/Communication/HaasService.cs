namespace HaasConnectorAPIService.Communication;

/// <summary>
/// HaasService
/// </summary>
public class HaasService : BackgroundService
{
    /// <summary>
    /// DataCollector interface
    /// </summary>
    internal HaasCommunication _dataCollector;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<HaasService> _logger;

    /// <summary>
    /// Gets or sets the _haasSettings
    /// Haas Settings
    /// </summary>
    private HaasSettings _haasSettings { get; set; }

    /// <summary>
    /// HaasService
    /// </summary>
    /// <param name="sqlLiteDbContext"></param>
    /// <param name="logger"></param>
    /// <param name="appSettings"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public HaasService(ILogger<HaasService> logger, IOptions<HaasSettings> appSettings)
    {
        _logger = logger;
        _haasSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(_haasSettings));

        _dataCollector = new HaasCommunication(appSettings.Value.SerialPort.Name, appSettings.Value.SerialPort.Baudrate,
            appSettings.Value.SerialPort.ReadTimeout, appSettings.Value.SerialPort.DataBits);
    }

    /// <summary>
    /// ExecuteAsync
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string data = string.Empty;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_dataCollector != null)
                data = _dataCollector.GetAllMachineData();

            // _db.HaasData.Add(data);
            //  await _db.SaveChangesAsync();

            await Task.Delay(_haasSettings.CollectionIntervalMs);
        }
    }
}

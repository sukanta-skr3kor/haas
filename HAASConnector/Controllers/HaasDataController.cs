namespace HaasConnectorService.Controllers;

/// <summary>
/// File APIs
/// </summary>
[ApiController]
[Route("api/v1/haas")]
public class HaasDataController : ApiControllerBase
{
    /// <summary>
    ///SqlLite Db
    /// </summary>
    private readonly AppDbContext _db;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<HaasDataController> _logger;

    /// <summary>
    /// Haas Serialport Settings
    /// </summary>
    private HaasSettings _haasSettings { get; set; }

    /// <summary>
    /// DataCollector interface
    /// </summary>
    internal HaasCommunication _dataCollector;

    /// <summary>
    /// HaasDataController
    /// </summary>
    /// <param name="sqlLiteDbContext"></param>
    /// <param name="logger"></param>
    /// <param name="appSettings"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public HaasDataController(AppDbContext sqlLiteDbContext, ILogger<HaasDataController> logger,
        IOptions<HaasSettings> appSettings)
    {
        _haasSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(_haasSettings));
        _db = sqlLiteDbContext ?? throw new ArgumentNullException(nameof(sqlLiteDbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _dataCollector = new HaasCommunication(appSettings.Value.SerialPort.Name, appSettings.Value.SerialPort.Baudrate,
            appSettings.Value.SerialPort.ReadTimeout, appSettings.Value.SerialPort.DataBits);
    }

    /// <summary>
    /// Service health
    /// </summary>
    /// <returns></returns>
    [HttpGet(@"machine/status")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    [AllowAnonymous]
    public IActionResult MachineStatus()
    {
        try
        {
            string status = _dataCollector.MachineStatus();
            _logger.LogInformation("Machine status collected");

            return this.Ok(status);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp.Message);
            return this.NotOkResponse("Offline");
        }
    }

    /// <summary>
    /// GetHaasData
    /// </summary>
    /// <param name="ParameterName"></param>
    /// <returns></returns>
    [HttpGet("machine/data")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetAllHaasData()
    {
        string machineData = "{}";
        try
        {
            machineData = _dataCollector.GetAllMachineData();
            _logger.LogInformation("All machine data collected");
        }
        catch (Exception exp)
        {
            //log exception
            _logger.LogError(exp.Message);
            return this.KnowOperationError(exp.Message);
        }

        return this.Ok(machineData);
    }

    /// <summary>
    /// GetHaasData
    /// </summary>
    /// <param name="ParameterName"></param>
    /// <returns></returns>
    [HttpGet(@"machine/{variable}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetHaasMachineVariable(int variable)
    {
        string variableData = "{}";

        if (variable <= 0)
            return this.KnowOperationError("No variable provided");

        try
        {
            variableData = _dataCollector.MachineVariableData(variable);
            _logger.LogInformation($"Machine variable : {variable} data collected");
        }
        catch (Exception exp)
        {
            //log exception
            _logger.LogError(exp.Message);
            return this.KnowOperationError(exp.Message);
        }

        return this.Ok(variableData);
    }

    /// <summary>
    ///  Get all log files
    /// </summary>
    /// <param name="numberofdays"></param>
    /// <returns></returns>
    [HttpGet("log/{numberofdays}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DownloadLogs(int numberofdays = 1)
    {
        List<string> logFilePaths = new List<string>();
        var zipFileName = "logfiles.zip";

        DirectoryInfo logDirectory = Reflections.GetRootRelativeDir("Logs");
        DirectoryInfo tempDirectory = Reflections.GetRootRelativeDir("temp");

        if (!tempDirectory.Exists)
        {
            tempDirectory.Create();
        }

        try
        {
            if (logDirectory.Exists)
            {
                var logFiles = logDirectory.GetFiles("*.log", SearchOption.TopDirectoryOnly)
                                           .OrderByDescending(f => f.LastWriteTimeUtc)
                                           .ToList();

                // Get last 5 distinct log dates
                var lastWriteDates = logFiles.Select(x => x.LastWriteTime.Date)
                                             .Distinct()
                                             .Take(numberofdays)
                                             .ToList();

                if (lastWriteDates.Any())
                {
                    logFilePaths = logFiles.Where(x => lastWriteDates.Contains(x.LastWriteTime.Date))
                                           .OrderByDescending(x => x.LastWriteTimeUtc)
                                           .Select(x => x.FullName)
                                           .ToList();

                    foreach (var file in logFiles.Where(x => lastWriteDates.Contains(x.LastWriteTime.Date)))
                    {
                        string destFilePath = Path.Combine(tempDirectory.FullName, file.Name);
                        file.CopyTo(destFilePath, overwrite: true);
                    }
                }
            }

            // Create ZIP file
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filePath in Directory.GetFiles(tempDirectory.FullName))
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(filePath);
                        var fileName = Path.GetFileName(filePath);
                        var zipEntry = archive.CreateEntry(fileName, CompressionLevel.Fastest);

                        using (var entryStream = zipEntry.Open())
                        using (var fileStream = new MemoryStream(fileBytes))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                _logger.LogInformation($"Machine logs collecetd.");

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/zip", zipFileName);
            }
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Error while generating log ZIP file.");
            return BadRequest("Error processing log files.");
        }
        finally
        {
            try
            {
                foreach (var file in tempDirectory.GetFiles())
                    file.Delete();
            }
            catch
            {
                //ignore errors
            }
        }
    }
}

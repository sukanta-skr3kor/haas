namespace HaasConnectorService.Models;

/// <summary>
/// Defines the <see cref="HaasSettings" />
public class HaasSettings
{
    public int CollectionIntervalMs { get; set; }

    public Logging Logging { get; set; }
    public SerialPortSettings SerialPort { get; set; }
}

/// <summary>
/// Log settings
/// </summary>
public class Logging
{
    public bool Enabled { get; set; }
    public bool EnableLogMonitoring { get; set; }
}

/// <summary>
/// SerialPort Settings
/// </summary>
public class SerialPortSettings
{
    public string Name { get; set; } = "COM1";
    public int Baudrate { get; set; } = 9600;
    public int DataBits { get; set; }
    public Parity Parity { get; set; }
    public StopBits StopBits { get; set; }
    public int ReadTimeout { get; set; } = 2000;
    public int WriteTimeout { get; set; } = 2000;
}

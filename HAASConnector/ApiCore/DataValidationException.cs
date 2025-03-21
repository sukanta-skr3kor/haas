namespace HaasConnectorService.ApiCore;

/// <summary>
/// Data validation exception
/// </summary>
[Serializable]
public class DataValidationException : ValidationException
{
    /// <summary>
    /// DataValidationException
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DataValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// DataValidationException
    /// </summary>
    /// <param name="message"></param>
    public DataValidationException(string message) : base(message) { }
}

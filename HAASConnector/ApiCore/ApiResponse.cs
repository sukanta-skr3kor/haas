namespace HaasConnectorService.ApiCore;

/// <summary>
/// General api response class
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates if api execution was successful
    /// </summary>
    [JsonProperty("isOk")]
    public bool IsOk { get; set; }

    /// <summary>
    /// Any response message
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }
}

/// <summary>
/// Api response message
/// </summary>
public class ApiContentResponse<T> : ApiResponse
{
    private bool v;
    private T? result;
    private string empty;

    public ApiContentResponse(bool v, T? result, string empty)
    {
        this.v = v;
        this.result = result;
        this.empty = empty;
    }

    /// <summary>
    /// Payload of the api
    /// </summary>
    [JsonProperty("content")]
    public T Content { get; set; }
}

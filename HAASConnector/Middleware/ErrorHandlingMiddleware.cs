namespace HaasConnectorService.Middleware;

/// <summary>
/// Exception handling middleware
/// </summary>
internal class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handle exception
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        try
        {
            context.Response.ContentType = "application/json";

            ApiResponse result = new ApiResponse() { IsOk = false, Message = "Internal Server Error" };

            if (exception is DataValidationException || exception is InvalidOperationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                result.Message = exception?.Message;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                result.Message = exception?.Message;
            }

            return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (Exception exp)
        {
            return Task.FromException(exp);
        }
    }
}

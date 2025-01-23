using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace TGB.AccountBE.API.Exceptions;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
            return false;

        var res = new
        {
            error = appException.Error,
            message = appException.Message
        };

        httpContext.Response.StatusCode = appException.StatusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(res), cancellationToken);
        return true;
    }
}

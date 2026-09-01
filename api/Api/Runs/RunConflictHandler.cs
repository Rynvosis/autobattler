using Microsoft.AspNetCore.Diagnostics;

namespace Api.Runs;

public sealed class RunConflictHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RunConflictException conflict) return false;

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        await context.Response.WriteAsJsonAsync(conflict.Stored, cancellationToken);

        return true;
    }
}

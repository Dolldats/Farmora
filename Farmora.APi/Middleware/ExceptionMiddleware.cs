using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Farmora.APi.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var (status, code, title, detail, errors) = Map(ex, context);

        logger.LogError(ex, "Unhandled exception {Code} traceId={TraceId}", code, traceId);

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var payload = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.com/{(int)status}",
            ["title"] = title,
            ["status"] = (int)status,
            ["detail"] = detail,
            ["traceId"] = traceId,
            ["code"] = code,
        };
        if (errors is not null)
            payload["errors"] = errors;
        if (env.IsDevelopment())
            payload["stackTrace"] = ex.ToString();

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static (HttpStatusCode Status, string Code, string Title, string Detail, object? Errors) Map(Exception ex, HttpContext context)
    {
        return ex switch
        {
            FluentValidation.ValidationException vex => (
                HttpStatusCode.BadRequest,
                "validation_failed",
                "Validation failed.",
                vex.Message,
                vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray() as object)
            ),
            ArgumentException aex => (
                HttpStatusCode.BadRequest,
                "bad_request",
                "Bad request.",
                aex.Message,
                null
            ),
            KeyNotFoundException kex => (
                HttpStatusCode.NotFound,
                "not_found",
                "Resource not found.",
                kex.Message,
                null
            ),
            DbUpdateConcurrencyException cex => (
                HttpStatusCode.Conflict,
                "concurrency_conflict",
                "Concurrency conflict.",
                cex.Message,
                null
            ),
            InvalidOperationException iex => (
                HttpStatusCode.Conflict,
                "conflict",
                "Conflict.",
                iex.Message,
                null
            ),
            UnauthorizedAccessException uex => context.User.Identity?.IsAuthenticated == true
                ? (HttpStatusCode.Forbidden, "forbidden", "Forbidden.", uex.Message, null)
                : (HttpStatusCode.Unauthorized, "unauthorized", "Unauthorized.", uex.Message, null),
            _ => (
                HttpStatusCode.InternalServerError,
                "internal_error",
                "An unexpected error occurred.",
                "An unexpected error occurred.",
                null
            ),
        };
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionMiddleware>();
}

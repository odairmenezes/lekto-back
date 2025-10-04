using Serilog;
using System.Diagnostics;
using Serilog.Events;

namespace CadPlus.Middleware;

/// <summary>
/// Middleware para logging detalhado de requests e responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        // Adicionar RequestId ao contexto para uso nos controllers
        context.Items["RequestId"] = requestId;
        
        // Adicionar propriedades ao contexto do Serilog usando LogContext
        using var logScope = Serilog.Context.LogContext.PushProperty("RequestId", requestId);
        using var pathScope = Serilog.Context.LogContext.PushProperty("Path", context.Request.Path);
        using var methodScope = Serilog.Context.LogContext.PushProperty("Method", context.Request.Method);

        // Log do início do request
        Log.Information("🔵 RequestStart - {Method} {Path} - UserAgent: {UserAgent} - IP: {ClientIP} - RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            context.Request.Headers.UserAgent.ToString(),
            context.Connection.RemoteIpAddress?.ToString(),
            requestId);

        // Contexto original para capturar response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "🔴 RequestError - {Method} {Path} - StatusCode: {StatusCode} - Duration: {Duration}ms - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                500,
                stopwatch.ElapsedMilliseconds,
                requestId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            // Restaurar o corpo da response
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            
            // Log do fim do request
            var logLevel = context.Response.StatusCode >= 400 ? LogEventLevel.Warning : LogEventLevel.Information;
            var eventName = context.Response.StatusCode >= 400 ? "🔴 RequestCompletedWithError" : "🟢 RequestCompleted";
            
            Log.Write(logLevel, "{EventName} - {Method} {Path} - StatusCode: {StatusCode} - Duration: {Duration}ms - ResponseLength: {ResponseLength}b - RequestId: {RequestId}",
                eventName,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseText.Length,
                requestId);

            // Log adicional para requests de autenticação
            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                Log.Information("🔐 AuthRequest - {Method} {Path} - StatusCode: {StatusCode} - Duration: {Duration}ms - RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    requestId);
            }
        }
    }
}

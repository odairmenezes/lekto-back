using CadPlus.DTOs;
using System.Net;
using System.Text.Json;

namespace CadPlus.Middleware;

/// <summary>
/// Middleware para captura e tratamento global de exceções
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado capturado pelo middleware");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>();

        switch (exception)
        {
            case UnauthorizedAccessException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Success = false;
                response.Message = "Acesso não autorizado";
                response.Errors = new List<string> { ex.Message };
                break;

            case KeyNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Success = false;
                response.Message = "Recurso não encontrado";
                response.Errors = new List<string> { ex.Message };
                break;

            case InvalidOperationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = "Operação inválida";
                response.Errors = new List<string> { ex.Message };
                break;

            case ArgumentNullException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = "Parâmetro obrigatório não fornecido";
                response.Errors = new List<string> { ex.Message };
                break;

            case ArgumentException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = "Parâmetro inválido";
                response.Errors = new List<string> { ex.Message };
                break;

            case NotImplementedException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                response.Success = false;
                response.Message = "Funcionalidade ainda não implementada";
                response.Errors = new List<string> { ex.Message };
                break;

            case TimeoutException ex:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Success = false;
                response.Message = "Timeout da operação";
                response.Errors = new List<string> { ex.Message };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Success = false;
                response.Message = "Erro interno do servidor";
                response.Errors = new List<string> { "Ocorreu um erro inesperado" };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Classe para configuração de exceções customizadas da aplicação
/// </summary>
public static class CustomExceptions
{
    /// <summary>
    /// Exceção específica para validação de dados
    /// </summary>
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message, List<string> errors) : base(message)
        {
            Errors = errors;
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }
    }

    /// <summary>
    /// Exceção específica para violação de regras de negócio
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção específica para dados duplicados
    /// </summary>
    public class DuplicateException : Exception
    {
        public string FieldName { get; }
        public object FieldValue { get; }

        public DuplicateException(string fieldName, object fieldValue, string? message = null) 
            : base(message ?? $"{fieldName} '{fieldValue}' já existe")
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }

    /// <summary>
    /// Exceção específica para problemas de autenticação/autorização
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }
}

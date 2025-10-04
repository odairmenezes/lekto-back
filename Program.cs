using Microsoft.EntityFrameworkCore;
using CadPlus.Data;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using CadPlus.Services;
using CadPlus.Services.Implementations;
using CadPlus.DTOs;
using CadPlus.Validators;
using CadPlus.Mappings;
using AutoMapper;
using Serilog.Events;
using CadPlus.Extensions;

// IMPORTANTE: Carregar .env ANTES de qualquer outra coisa
EnvironmentExtensions.LoadEnvironmentFile();

// Configurar porta da aplicação
var port = EnvironmentExtensions.GetEnvironmentVariableOrDefault("API_PORT", "7001");
var url = EnvironmentExtensions.GetEnvironmentVariableOrDefault("API_URL", "http://localhost:7001");

// Configurar argumentos para incluir URLs
var argsWithUrls = new[] { $"--urls={url}" };
var builder = WebApplication.CreateBuilder(argsWithUrls);

// Configurar HTTPS redirection para false em desenvolvimento
if (!builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = null;
    });
}

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "CadPlus-ERP")
    .Enrich.WithProperty("Version", "1.0.0")
    .CreateLogger();

builder.Host.UseSerilog();

// Registrar serviços adicionais necessários para controllers
// JWT Configuration - usando variáveis de ambiente
var jwtSecret = EnvironmentExtensions.GetEnvironmentVariableOrDefault("JWT_SECRET_KEY", "CadPlus_Super_Secret_Key_Minimum_256_Bits_Development_Only_Safe_Key_12345678");
var jwtIssuer = EnvironmentExtensions.GetEnvironmentVariableOrDefault("JWT_ISSUER", "CadPlusERP");
var jwtAudience = EnvironmentExtensions.GetEnvironmentVariableOrDefault("JWT_AUDIENCE", "CadPlusFrontend");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Registros de interfaces e serviços
builder.Services.AddScoped<IAuthService, CadPlus.Services.Implementations.AuthService>();
builder.Services.AddScoped<IUserService, CadPlus.Services.Implementations.UserService>();
builder.Services.AddScoped<IPasswordService, CadPlus.Services.Implementations.PasswordService>();
builder.Services.AddScoped<ICpfValidationService, CadPlus.Services.Implementations.CpfValidationService>();

// FluentValidation
builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserValidator>();
builder.Services.AddScoped<IValidator<CreateAddressDto>, CreateAddressValidator>();
builder.Services.AddScoped<IValidator<UpdateAddressDto>, UpdateAddressValidator>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(UserProfile));

// Add services
builder.Services.AddDbContext<CadPlusDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
	options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

var app = builder.Build();

// Configure pipeline
app.UseRouting();

// Middleware de logging customizado temporariamente comentado para debug
// app.UseMiddleware<CadPlus.Middleware.RequestLoggingMiddleware>();

// Adicionar middleware de logging do Serilog (como backup)
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "SerilogRequest - {RequestMethod} {RequestPath} retornou {StatusCode} ({ElapsedMilliseconds} ms)";
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug; // Debug level para não duplicar logs
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

// Adicionar autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Message = "Cad+ ERP API está funcionando!" 
}));

app.MapGet("/", () => Results.Ok(new { 
    Message = "Cad+ ERP API", 
    Version = "1.0.0",
    Status = "Online",
    Timestamp = DateTime.UtcNow 
}));

Log.Information("🚀 Cad+ ERP API iniciada!");
Log.Information($"📖 API Status: {url}/health");
Log.Information($"📊 Base URL: {url}/");

try
{
    app.Run(url);
}
catch (Exception ex)
{
    Log.Fatal(ex, "O aplicativo falhou ao inicializar!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
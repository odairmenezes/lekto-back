using Microsoft.EntityFrameworkCore;
using CadPlus.Data;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using CadPlus.Services;
using CadPlus.Services.Interfaces;
using CadPlus.Services.Implementations;
using CadPlus.DTOs;
using CadPlus.Validators;
using CadPlus.Mappings;
using AutoMapper;
using Serilog.Events;

// Criar builder da aplicação
var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "CadPlus-ERP")
    .Enrich.WithProperty("Version", "1.0.0")
    .CreateLogger();

builder.Host.UseSerilog();

// Registrar serviços adicionais necessários para controllers
// JWT Configuration - usando configuração nativa do .NET
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"] ?? 
                Environment.GetEnvironmentVariable("JwtSettings__SecretKey") ?? 
                "CadPlus_Super_Secret_Key_Minimum_256_Bits_Development_Only_Safe_Key_12345678";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? 
                Environment.GetEnvironmentVariable("JwtSettings__Issuer") ?? 
                "CadPlusERP";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? 
                  Environment.GetEnvironmentVariable("JwtSettings__Audience") ?? 
                  "CadPlusFrontend";

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

// Configuração de CORS - usando configuração nativa do .NET
var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? 
                 Environment.GetEnvironmentVariable("CorsSettings__AllowedOrigins")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? 
                 new[] { "*" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CadPlusPolicy", policy =>
    {
        if (corsOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetIsOriginAllowed(origin => true)
                  .AllowCredentials();
        }
    });
});

builder.Services.AddAuthorization();

// Registros de interfaces e serviços
builder.Services.AddScoped<IAuthService, CadPlus.Services.Implementations.AuthService>();
builder.Services.AddScoped<IUserService, CadPlus.Services.Implementations.UserService>();
builder.Services.AddScoped<IPasswordService, CadPlus.Services.Implementations.PasswordService>();
builder.Services.AddScoped<ICpfValidationService, CadPlus.Services.Implementations.CpfValidationService>();
builder.Services.AddScoped<IAuditService, CadPlus.Services.Implementations.AuditService>();

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

// Habilitar CORS
app.UseCors("CadPlusPolicy");

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
Log.Information($"📖 API Status: {builder.Configuration["ApiSettings:Url"] ?? "http://localhost:7001"}/health");
Log.Information($"📊 Base URL: {builder.Configuration["ApiSettings:Url"] ?? "http://localhost:7001"}/");

try
{
    app.Run();
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
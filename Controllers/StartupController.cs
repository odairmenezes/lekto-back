using Microsoft.AspNetCore.Mvc;
using CadPlus.Data;
using Microsoft.EntityFrameworkCore;
using CadPlus.Services.Implementations;
using CadPlus.DTOs;

namespace CadPlus.Controllers;

/// <summary>
/// Controller para operações de inicialização do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StartupController : ControllerBase
{
    private readonly CadPlusDbContext _context;
    private readonly ILogger<StartupController> _logger;

    public StartupController(CadPlusDbContext context, ILogger<StartupController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa o banco de dados criando tabelas e dados básicos
    /// </summary>
    [HttpPost("init")]
    public async Task<ActionResult> InitializeDatabase()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        using var logScope = _logger.BeginScope("RequestId: {RequestId}, Action: InitializeDatabase", requestId);
        
        try
        {
            _logger.LogInformation("BeginInit - Inicializando banco de dados - RequestId: {RequestId}", requestId);

            // Garantir que as tabelas existam
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("DatabaseCreated - Tabelas criadas com sucesso - RequestId: {RequestId}", requestId);

            // Verificar se já existe um usuário admin
            var adminExists = await _context.Users.AnyAsync(u => u.Email == "admin@cadplus.com.br");
            
            if (!adminExists)
            {
                _logger.LogInformation("CreatingAdminUser - Criando usuário admin padrão - RequestId: {RequestId}", requestId);
                
                // Criar usuário admin padrão
                var passwordService = new PasswordService();
                var passwordHash = passwordService.HashPassword("Admin123!@#");
                
                var adminUser = new Models.User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Administrador",
                    LastName = "Sistema",
                    Email = "admin@cadplus.com.br",
                    Cpf = "12345678901", // CPF fictício
                    Phone = "(11) 99999-9999",
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
                
                stopwatch.Stop();
                _logger.LogInformation("InitSuccess - Banco inicializado com sucesso - Admin criado - Duration: {Duration}ms - RequestId: {RequestId}", 
                    stopwatch.ElapsedMilliseconds, requestId);
            }
            else
            {
                stopwatch.Stop();
                _logger.LogInformation("InitSuccess - Banco já inicializado - Admin já existe - Duration: {Duration}ms - RequestId: {RequestId}", 
                    stopwatch.ElapsedMilliseconds, requestId);
            }

            return Ok(new { 
                Success = true, 
                Message = "Banco de dados inicializado com sucesso!",
                HasAdmin = true,
                Duration = stopwatch.ElapsedMilliseconds,
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "InitError - Erro durante inicialização do banco - Duration: {Duration}ms - RequestId: {RequestId}", 
                stopwatch.ElapsedMilliseconds, requestId);
            
            return StatusCode(500, new { 
                Success = false, 
                Message = "Erro durante inicialização do banco de dados",
                Duration = stopwatch.ElapsedMilliseconds,
                RequestId = requestId
            });
        }
    }

    /// <summary>
    /// Verifica se o banco de dados está funcionando
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetDatabaseStatus()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var userCount = await _context.Users.CountAsync();
            
            return Ok(new { 
                Success = true, 
                DatabaseConnection = canConnect,
                UserCount = userCount,
                Message = canConnect ? "Banco funcionando" : "Problema na conexão"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DatabaseStatusError - Erro ao verificar status do banco");
            return StatusCode(500, new { 
                Success = false, 
                Message = "Erro ao verificar status do banco",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Adiciona usuários de teste para desenvolvimento
    /// </summary>
    [HttpPost("add-test-user")]
    public async Task<ActionResult> AddTestUser([FromBody] AddTestUserDto testUserDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        using var logScope = _logger.BeginScope("RequestId: {RequestId}, Action: AddTestUser", requestId);
        
        try
        {
            _logger.LogInformation("BeginAddTestUser - Criando usuário de teste - Email: {Email}, RequestId: {RequestId}", 
                testUserDto.Email, requestId);

            // Verificar se o usuário já existe
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == testUserDto.Email);
            if (existingUser != null)
            {
                stopwatch.Stop();
                _logger.LogWarning("TestUserAlreadyExists - Usuário de teste já existe - Email: {Email}, Duration: {Duration}ms, RequestId: {RequestId}", 
                    testUserDto.Email, stopwatch.ElapsedMilliseconds, requestId);
                
                return Conflict(new { 
                    Success = false, 
                    Message = "Usuário já existe",
                    Email = testUserDto.Email,
                    Duration = stopwatch.ElapsedMilliseconds,
                    RequestId = requestId
                });
            }

            // Criar usuário de teste
            var passwordService = new CadPlus.Services.Implementations.PasswordService();
            var passwordHash = passwordService.HashPassword(testUserDto.Password);
            
            var testUser = new Models.User
            {
                Id = Guid.NewGuid(),
                FirstName = testUserDto.FirstName,
                LastName = testUserDto.LastName,
                Email = testUserDto.Email,
                Cpf = testUserDto.Cpf,
                Phone = testUserDto.Phone,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();
            
            stopwatch.Stop();
            _logger.LogInformation("TestUserCreated - Usuário de teste criado - Email: {Email}, UserId: {UserId}, Duration: {Duration}ms, RequestId: {RequestId}", 
                testUserDto.Email, testUser.Id, stopwatch.ElapsedMilliseconds, requestId);

            return Ok(new { 
                Success = true, 
                Message = "Usuário de teste criado com sucesso!",
                UserId = testUser.Id,
                Email = testUser.Email,
                Duration = stopwatch.ElapsedMilliseconds,
                RequestId = requestId,
                LoginData = new {
                    Email = testUser.Email,
                    Password = testUserDto.Password
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "TestUserError - Erro durante criação de usuário de teste - Email: {Email}, Duration: {Duration}ms, RequestId: {RequestId}", 
                testUserDto.Email, stopwatch.ElapsedMilliseconds, requestId);
            
            return StatusCode(500, new { 
                Success = false, 
                Message = "Erro durante criação de usuário de teste",
                Duration = stopwatch.ElapsedMilliseconds,
                RequestId = requestId
            });
        }
    }
}

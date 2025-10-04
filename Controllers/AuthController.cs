using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FluentValidation;
using CadPlus.DTOs;
using CadPlus.Services;
using Serilog;

namespace CadPlus.Controllers;

/// <summary>
/// Controller responsável pela autenticação de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IValidator<LoginDto> loginValidator,
        IValidator<RegisterDto> registerValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _logger = logger;
    }

    /// <summary>
    /// Realizar login no sistema
    /// </summary>
    /// <param name="loginDto">Dados do login</param>
    /// <returns>Token de acesso e informações do usuário</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            _logger.LogInformation("Login attempt - Email: {Email}", loginDto.Email);

            // Usar o método LoginAsync que já está funcionando conforme os logs
            var result = await _authService.LoginAsync(loginDto);
            
            return Ok(new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Login realizado com sucesso",
                Data = result
            });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Login unauthorized - Email: {Email}", loginDto.Email);
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Credenciais inválidas",
                Errors = new List<string> { "E-mail ou senha incorretos" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error - Email: {Email}", loginDto.Email);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Registrar novo usuário no sistema
    /// </summary>
    /// <param name="registerDto">Dados de registro</param>
    /// <returns>Token de acesso e informações do usuário</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        using var logScope = _logger.BeginScope("RequestId: {RequestId}, Action: Register", requestId);
        
        try
        {
            _logger.LogInformation("BeginRegister - Tentativa de registro para e-mail: {Email}, UserAgent: {UserAgent}, IP: {ClientIP}", 
                registerDto.Email, 
                HttpContext.Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Validar entrada
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("RegisterValidationFailed - Dados inválidos para e-mail: {Email}, Errors: {Errors}, Duration: {Duration}ms", 
                    registerDto.Email, 
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    stopwatch.ElapsedMilliseconds);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dados de cadastro inválidos",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            _logger.LogDebug("RegisterValidation - Validação bem-sucedida para e-mail: {Email}", registerDto.Email);

            // Registrar usuário
            var result = await _authService.RegisterAsync(registerDto);
            
            stopwatch.Stop();
            _logger.LogInformation("RegisterSuccess - Novo usuário cadastrado com sucesso: {Email}, UserId: {UserId}, Duration: {Duration}ms", 
                registerDto.Email, result.User.Id, stopwatch.ElapsedMilliseconds);
            
            return CreatedAtAction(nameof(Register), new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Usuário cadastrado com sucesso",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("RegisterConflict - Dados duplicados para e-mail: {Email}, Reason: {Reason}, Duration: {Duration}ms", 
                registerDto.Email, ex.Message, stopwatch.ElapsedMilliseconds);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = "Dados já cadastrados",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "RegisterError - Erro durante cadastro para e-mail: {Email}, Duration: {Duration}ms", 
                registerDto.Email, stopwatch.ElapsedMilliseconds);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Validar token de acesso atual
    /// </summary>
    /// <returns>Status da validação</returns>
    [HttpPost("validate")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<bool>> Validate()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var userId = User.FindFirst("sub")?.Value;
        
        using var logScope = _logger.BeginScope("Action: Validate, UserId: {UserId}", userId);
        
        try
        {
            _logger.LogInformation("BeginValidate - Validação de token solicitada pelo usuário: {UserId}, UserAgent: {UserAgent}, IP: {ClientIP}", 
                userId,
                HttpContext.Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString());

            stopwatch.Stop();
            _logger.LogInformation("ValidateSuccess - Token válido para usuário: {UserId}, Duration: {Duration}ms", 
                userId, stopwatch.ElapsedMilliseconds);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Token válido",
                Data = true
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "ValidateError - Erro durante validação de token para usuário: {UserId}, Duration: {Duration}ms", 
                userId, stopwatch.ElapsedMilliseconds);
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Token inválido",
                Errors = new List<string> { "Token expirado ou inválido" }
            });
        }
    }

    /// <summary>
    /// Renovar token de acesso usando refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Dados do refresh token</param>
    /// <returns>Novo token de acesso</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            _logger.LogInformation("Tentativa de renovação de token");

            if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Refresh token é obrigatório",
                    Errors = new List<string> { "Refresh token não fornecido" }
                });
            }

            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            
            _logger.LogInformation("Token renovado com sucesso para usuário: {UserId}", result.User.Id);
            
            return Ok(new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Token renovado com sucesso",
                Data = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Tentativa de renovação de token com refresh token inválido: {Message}", ex.Message);
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Refresh token inválido",
                Errors = new List<string> { "Refresh token expirado ou inválido" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante renovação de token");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Obter informações do usuário logado
    /// </summary>
    /// <returns>Informações do usuário</returns>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<UserDto>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token inválido",
                    Errors = new List<string> { "ID do usuário não encontrado no token" }
                });
            }

            var userDto = new UserDto
            {
                Id = userId,
                FirstName = User.FindFirst("given_name")?.Value ?? "",
                LastName = User.FindFirst("family_name")?.Value ?? "",
                Email = User.FindFirst("email")?.Value ?? "",
                Cpf = User.FindFirst("cpf")?.Value ?? "",
                IsActive = bool.Parse(User.FindFirst("IsActive")?.Value ?? "false")
            };

            _logger.LogInformation("Informações do usuário atual obtidas: {UserId}", userId);

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Informações do usuário obtidas com sucesso",
                Data = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações do usuário atual");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado." }
            });
        }
    }
}

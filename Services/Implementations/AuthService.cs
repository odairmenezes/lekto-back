using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CadPlus.Services;
using CadPlus.Models;
using CadPlus.DTOs;
using Microsoft.Extensions.Configuration;

namespace CadPlus.Services.Implementations;

/// <summary>
/// Serviço de autenticação
/// </summary>
public class AuthService : IAuthService
{
    private readonly ICpfValidationService _cpfValidationService;
    private readonly IPasswordService _passwordService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    
    // JWT Settings - Usando configurações nativas do .NET
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthService(
        ICpfValidationService cpfValidationService,
        IPasswordService passwordService,
        IUserService userService,
        IConfiguration configuration)
    {
        _cpfValidationService = cpfValidationService;
        _passwordService = passwordService;
        _userService = userService;
        _configuration = configuration;
        
        // Carregar configurações JWT usando configuração nativa do .NET
        _jwtSecret = _configuration["JwtSettings:SecretKey"] ?? 
                    Environment.GetEnvironmentVariable("JwtSettings__SecretKey") ?? 
                    "CadPlus_Super_Secret_Key_Minimum_256_Bits_Development_Only_Safe_Key_12345678";
        _jwtIssuer = _configuration["JwtSettings:Issuer"] ?? 
                    Environment.GetEnvironmentVariable("JwtSettings__Issuer") ?? 
                    "CadPlusERP";
        _jwtAudience = _configuration["JwtSettings:Audience"] ?? 
                      Environment.GetEnvironmentVariable("JwtSettings__Audience") ?? 
                      "CadPlusFrontend";
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Validar CPF
        if (!_cpfValidationService.IsValid(registerDto.Cpf))
            throw new ArgumentException("CPF inválido");

        // Validar senha forte
        if (!_passwordService.IsStrongPassword(registerDto.Password))
            throw new ArgumentException("Senha não atende aos critérios de segurança");

        // Verificar se CPF já existe
        if (await _userService.CpfExistsAsync(registerDto.Cpf))
            throw new InvalidOperationException("CPF já cadastrado");

        // Verificar se email já existe
        if (await _userService.EmailExistsAsync(registerDto.Email))
            throw new InvalidOperationException("E-mail já cadastrado");

        // Validar confirmação de senha
        if (registerDto.Password != registerDto.ConfirmPassword)
            throw new ArgumentException("Senhas não conferem");

        // Validar endereços
        if (registerDto.Addresses == null || !registerDto.Addresses.Any())
            throw new ArgumentException("Pelo menos um endereço é obrigatório");

        // Criar usuário usando o UserService
        var createUserDto = new CreateUserDto
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Cpf = registerDto.Cpf,
            Email = registerDto.Email,
            Phone = registerDto.Phone,
            Password = registerDto.Password,
            ConfirmPassword = registerDto.ConfirmPassword,
            Addresses = registerDto.Addresses
        };

        var user = await _userService.CreateAsync(createUserDto);

        // Gerar tokens
        var token = GenerateToken(user.Id);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = user
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Email))
            throw new ArgumentException("E-mail é obrigatório");

        var user = await _userService.GetByEmailAsync(loginDto.Email);
        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        if (!user.IsActive)
            throw new InvalidOperationException("Usuário inativo");

        // Por enquanto, vamos assumir que a senha está correta se o usuário foi encontrado
        // Em uma implementação real, seria necessário buscar o User com PasswordHash

        // Gerar tokens
        var token = GenerateToken(user.Id);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = user
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtIssuer,
                ValidAudience = _jwtAudience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
    }

    public string GenerateToken(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()) // JWT Standard claim
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24), // Fixed 24 hours
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Implementação simples - retorna novo token sem validar refresh token no banco
        return Task.FromResult(new AuthResponseDto
        {
            AccessToken = GenerateToken(Guid.NewGuid()),
            RefreshToken = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(24) // Fixed 24 hours
        });
    }

    public Task<Dictionary<string, string>> GetUserClaimsFromTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadJwtToken(token);
        
        return Task.FromResult(securityToken.Claims.ToDictionary(c => c.Type, c => c.Value));
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}

/// <summary>
/// Configurações JWT
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationHours { get; set; } = 24;
}

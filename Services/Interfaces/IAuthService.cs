using CadPlus.DTOs;

namespace CadPlus.Services;

/// <summary>
/// Serviço responsável pela autenticação de usuários
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Realiza login do usuário
    /// </summary>
    /// <param name="loginDto">Dados de login</param>
    /// <returns>Token de acesso e informações do usuário</returns>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    
    /// <summary>
    /// Registra novo usuário
    /// </summary>
    /// <param name="registerDto">Dados de registro</param>
    /// <returns>Token de acesso e informações do usuário</returns>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    
    /// <summary>
    /// Valida se o token é válido
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <returns>True se válido, false caso contrário</returns>
    Task<bool> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Renova token de acesso usando refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>Novo token de acesso</returns>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Gera novo token JWT para o usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Token JWT</returns>
    string GenerateToken(Guid userId);
    
    /// <summary>
    /// Extrai informações do usuário do token JWT
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <returns>Claims do usuário</returns>
    Task<Dictionary<string, string>> GetUserClaimsFromTokenAsync(string token);
}

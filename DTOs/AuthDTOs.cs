using System.ComponentModel.DataAnnotations;

namespace CadPlus.DTOs;

/// <summary>
/// DTO para login de usuário
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO para registro de usuário
/// </summary>
public class RegisterDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Nome deve ter entre 4 e 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Sobrenome é obrigatório")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Sobrenome deve ter entre 1 e 100 caracteres")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(11, ErrorMessage = "CPF deve ter 11 dígitos")]
    public string Cpf { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefone é obrigatório")]
    [StringLength(15)]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare(nameof(Password), ErrorMessage = "Senhas não conferem")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Pelo menos um endereço é obrigatório")]
    [MinLength(1, ErrorMessage = "Pelo menos um endereço é obrigatório")]
    public List<CreateAddressDto> Addresses { get; set; } = new();
}

/// <summary>
/// DTO para resposta de autenticação
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// DTO para refresh token
/// </summary>
public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token é obrigatório")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO base para respostas da API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para lista paginada
/// </summary>
public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

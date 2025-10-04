using System.ComponentModel.DataAnnotations;

namespace CadPlus.DTOs;

/// <summary>
/// DTO para retorno de dados do usuário
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AddressDto> Addresses { get; set; } = new();
}

/// <summary>
/// DTO para criação de usuário
/// </summary>
public class CreateUserDto
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
    [StringLength(254, ErrorMessage = "E-mail deve ter no máximo 254 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefone é obrigatório")]
    [StringLength(15, ErrorMessage = "Telefone deve ter no máximo 15 caracteres")]
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
/// DTO para atualização de usuário
/// </summary>
public class UpdateUserDto
{
    [StringLength(100, MinimumLength = 4)]
    public string? FirstName { get; set; }
    
    [StringLength(100, MinimumLength = 1)]
    public string? LastName { get; set; }
    
    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }
    
    [StringLength(15)]
    public string? Phone { get; set; }
    
    [MinLength(8)]
    public string? Password { get; set; }
}

/// <summary>
/// DTO para retorno de endereço
/// </summary>
public class AddressDto
{
    public Guid Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Number { get; set; }
    public string? Neighborhood { get; set; }
    public string? Complement { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

/// <summary>
/// DTO para criação de endereço
/// </summary>
public class CreateAddressDto
{
    [Required(ErrorMessage = "Rua é obrigatória")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Rua deve ter entre 5 e 200 caracteres")]
    public string Street { get; set; } = string.Empty;
    
    [StringLength(15, ErrorMessage = "Número deve ter no máximo 15 caracteres")]
    public string? Number { get; set; }
    
    [StringLength(50, ErrorMessage = "Bairro deve ter no máximo 50 caracteres")]
    public string? Neighborhood { get; set; }
    
    [StringLength(100)]
    public string? Complement { get; set; }
    
    [Required(ErrorMessage = "Cidade é obrigatória")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Cidade deve ter entre 2 e 100 caracteres")]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Estado é obrigatório")]
    [StringLength(2, ErrorMessage = "Estado deve ser a sigla com 2 caracteres")]
    public string State { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "CEP é obrigatório")]
    [StringLength(10)]
    public string ZipCode { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Country { get; set; } = "Brasil";
    
    public bool IsPrimary { get; set; } = false;
}

/// <summary>
/// DTO para atualização de endereço
/// </summary>
public class UpdateAddressDto
{
    [StringLength(200, MinimumLength = 5)]
    public string? Street { get; set; }
    
    [StringLength(15)]
    public string? Number { get; set; }
    
    [StringLength(50)]
    public string? Neighborhood { get; set; }
    
    [StringLength(100)]
    public string? Complement { get; set; }
    public string? City { get; set; }
    
    [StringLength(2)]
    public string? State { get; set; }
    
    [StringLength(10)]
    public string? ZipCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    public bool? IsPrimary { get; set; }
}

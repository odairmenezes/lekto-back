using System.ComponentModel.DataAnnotations;

namespace CadPlus.DTOs;

/// <summary>
/// DTO para criação de usuários de teste
/// </summary>
public class AddTestUserDto
{
    /// <summary>
    /// Primeiro nome do usuário
    /// </summary>
    [Required(ErrorMessage = "O primeiro nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O primeiro nome deve ter no máximo 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Sobrenome do usuário
    /// </summary>
    [Required(ErrorMessage = "O sobrenome é obrigatório")]
    [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(254, ErrorMessage = "O email deve ter no máximo 254 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// CPF do usuário (apenas números)
    /// </summary>
    [Required(ErrorMessage = "O CPF é obrigatório")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter exatamente 11 números")]
    [RegularExpression("^[0-9]{11}$", ErrorMessage = "CPF deve conter apenas números")]
    public string Cpf { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do usuário
    /// </summary>
    [Required(ErrorMessage = "O telefone é obrigatório")]
    [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 50 caracteres")]
    public string Password { get; set; } = string.Empty;
}



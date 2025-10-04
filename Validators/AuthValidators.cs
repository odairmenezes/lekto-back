using FluentValidation;
using CadPlus.DTOs;

namespace CadPlus.Validators;

/// <summary>
/// Validador para LoginDto
/// </summary>
public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(1).WithMessage("Senha é obrigatória");
    }
}

/// <summary>
/// Validador para RegisterDto
/// </summary>
public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(4).WithMessage("Nome deve ter no mínimo 4 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Sobrenome é obrigatório")
            .MinimumLength(1).WithMessage("Sobrenome deve ter no mínimo 1 caractere")
            .MaximumLength(100).WithMessage("Sobrenome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Length(11, 14).WithMessage("CPF deve ter entre 11 e 14 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .MaximumLength(15).WithMessage("Telefone deve ter no máximo 15 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
            .Equal(x => x.Password).WithMessage("Senhas não conferem");

        RuleFor(x => x.Addresses)
            .NotEmpty().WithMessage("Pelo menos um endereço é obrigatório")
            .Must(addresses => addresses != null && addresses.Any()).WithMessage("Pelo menos um endereço é obrigatório");
    }
}




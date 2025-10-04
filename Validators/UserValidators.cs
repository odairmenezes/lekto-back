using FluentValidation;
using CadPlus.DTOs;

namespace CadPlus.Validators;

/// <summary>
/// Validador para CreateUserDto  
/// </summary>
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
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
            .NotEmpty().WithMessage("CPF é obrigatório");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .MaximumLength(15).WithMessage("Telefone deve ter no máximo 15 caracteres");
    }
}

/// <summary>
/// Validador para UpdateUserDto
/// </summary>
public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(4).WithMessage("Nome deve ter no mínimo 4 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(1).WithMessage("Sobrenome deve ter no mínimo 1 caractere")
            .MaximumLength(100).WithMessage("Sobrenome deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Phone)
            .MaximumLength(15).WithMessage("Telefone deve ter no máximo 15 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}




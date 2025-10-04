using CadPlus.DTOs;
using FluentValidation;

namespace CadPlus.Validators;

/// <summary>
/// Validator para criação de endereços
/// </summary>
public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Rua é obrigatória")
            .MaximumLength(200)
            .WithMessage("Rua deve ter no máximo 200 caracteres");

        RuleFor(x => x.Number)
            .MaximumLength(15)
            .WithMessage("Número deve ter no máximo 15 caracteres");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(50)
            .WithMessage("Bairro deve ter no máximo 50 caracteres");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Cidade é obrigatória")
            .MaximumLength(100)
            .WithMessage("Cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("Estado é obrigatório")
            .MaximumLength(2)
            .WithMessage("Estado deve ter no máximo 2 caracteres");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("CEP é obrigatório")
            .Matches(@"^\d{5}-?\d{3}$")
            .WithMessage("CEP deve estar no formato 12345-678");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("País deve ter no máximo 100 caracteres");

        RuleFor(x => x.Complement)
            .MaximumLength(200)
            .WithMessage("Complemento deve ter no máximo 200 caracteres");
    }
}

/// <summary>
/// Validator para atualização de endereços
/// </summary>
public class UpdateAddressValidator : AbstractValidator<UpdateAddressDto>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Street))
            .WithMessage("Rua não pode estar vazia")
            .MaximumLength(200)
            .WithMessage("Rua deve ter no máximo 200 caracteres");

        RuleFor(x => x.Number)
            .MaximumLength(15)
            .WithMessage("Número deve ter no máximo 15 caracteres");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(50)
            .WithMessage("Bairro deve ter no máximo 50 caracteres");

        RuleFor(x => x.City)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("Cidade não pode estar vazia")
            .MaximumLength(100)
            .WithMessage("Cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.State)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.State))
            .WithMessage("Estado não pode estar vazio")
            .MaximumLength(2)
            .WithMessage("Estado deve ter no máximo 2 caracteres");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.ZipCode))
            .WithMessage("CEP não pode estar vazio")
            .Matches(@"^\d{5}-?\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.ZipCode))
            .WithMessage("CEP deve estar no formato 12345-678");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("País deve ter no máximo 100 caracteres");

        RuleFor(x => x.Complement)
            .MaximumLength(200)
            .WithMessage("Complemento deve ter no máximo 200 caracteres");
    }
}

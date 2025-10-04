using FluentValidation;
using CadPlus.DTOs;
using CadPlus.Services;
using System.Text.RegularExpressions;

namespace CadPlus.Validators;

/// <summary>
/// Validador FluentValidation para CreateUserDto
/// </summary>
public class UserValidator : AbstractValidator<CreateUserDto>
{
    private readonly ICpfValidationService _cpfValidationService;
    private readonly IPasswordService _passwordService;

    public UserValidator(ICpfValidationService cpfValidationService, IPasswordService passwordService)
    {
        _cpfValidationService = cpfValidationService;
        _passwordService = passwordService;

        ConfigureRules();
    }

    private void ConfigureRules()
    {
        // Validação do Nome
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(4).WithMessage("Nome deve ter no mínimo 4 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Nome deve conter apenas letras e espaços")
            .Must(NotBeSequentialChars).WithMessage("Nome não deve ter caracteres em sequência");

        // Validação do Sobrenome
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Sobrenome é obrigatório")
            .MinimumLength(1).WithMessage("Sobrenome deve ter no mínimo 1 caractere")
            .MaximumLength(100).WithMessage("Sobrenome deve ter no máximo 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Sobrenome deve conter apenas letras e espaços");

        // Validação do CPF
        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Must(cpf => _cpfValidationService.IsValid(cpf)).WithMessage("CPF inválido")
            .DependentRules(() =>
            {
                RuleFor(x => x.Cpf)
                    .Length(11, 14).WithMessage("CPF deve ter entre 11 e 14 caracteres");
            });

        // Validação do E-mail
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres")
            .Must(BeValidEmailDomain).WithMessage("Domínio do e-mail inválido")
            .Must(NotHaveInvalidChars).WithMessage("E-mail contém caracteres inválidos");

        // Validação do Telefone
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^[1-9]{2}[9]?[0-9]{4}[0-9]{4}$").WithMessage("Telefone inválido")
            .Must(BeValidDdd).WithMessage("DDD inválido")
            .Must(BeValidPhoneNumber).WithMessage("Número de telefone inválido");

        // Validação da Senha
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .Must(password => _passwordService.IsStrongPassword(password))
            .WithMessage("Senha não atende aos critérios de segurança")
            .DependentRules(() =>
            {
                RuleFor(x => x.Password).Custom((password, context) =>
                {
                    var errors = GetPasswordValidationErrors(password);
                    foreach (var error in errors)
                    {
                        context.AddFailure(error);
                    }
                });
            })
            .Must(NotContainPersonalInfo).WithMessage("Senha não pode conter informações pessoais");

        // Validação da Confirmação de Senha
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
            .Equal(x => x.Password).WithMessage("Senhas não conferem");

        // Validação dos Endereços
        RuleFor(x => x.Addresses)
            .NotEmpty().WithMessage("Pelo menos um endereço é obrigatório")
            .Must(HaveValidAddresses).WithMessage("Endereços inválidos")
            .Must(HaveOnlyOnePrimary).WithMessage("Apenas um endereço pode ser principal");

        // Validação individual de cada endereço
        RuleForEach(x => x.Addresses)
            .SetValidator(new AddressValidator());
    }

    /// <summary>
    /// Verifica se não são caracteres em sequência
    /// </summary>
    private static bool NotBeSequentialChars(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true;

        return !Regex.IsMatch(name, @"(.)\1{2,}");
    }

    /// <summary>
    /// Verifica se o domínio do e-mail é válido
    /// </summary>
    private static bool BeValidEmailDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var domain = email.Split('@').LastOrDefault();
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        return domain.Contains('.') && domain.Length >= 3;
    }

    /// <summary>
    /// Verifica se não contém caracteres inválidos no e-mail
    /// </summary>
    private static bool NotHaveInvalidChars(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return !email.Contains("..") && !email.StartsWith(".") && !email.EndsWith(".");
    }

    /// <summary>
    /// Verifica se o DDD é válido
    /// </summary>
    private static bool BeValidDdd(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 2)
            return false;

        var ddd = phone.Length >= 10 ? phone[..2] : phone[..2];
        
        // DDDs válidos no Brasil (11-99, exceto 07)
        return int.TryParse(ddd, out var dddNumber) && 
               dddNumber >= 11 && dddNumber != 70 && dddNumber <= 99;
    }

    /// <summary>
    /// Verifica se o número de telefone é válido
    /// </summary>
    private static bool BeValidPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove espaços, parênteses e hífens
        var cleanPhone = phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
        
        // Deve ter 10 ou 11 dígitos
        return cleanPhone.Length == 10 || cleanPhone.Length == 11;
    }

    /// <summary>
    /// Verifica se a senha não contém informações pessoais
    /// </summary>
    private static bool NotContainPersonalInfo(CreateUserDto dto, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var passwordLower = password.ToLower();
        
        // Verifica se contém informações do usuário
        var personalInfo = new[]
        {
            dto.FirstName.ToLower(),
            dto.LastName.ToLower(),
            dto.Cpf.ToLower(),
            dto.Email.ToLower(),
            dto.Phone.ToLower()
        }.Where(info => info.Length >= 3);

        return !personalInfo.Any(info => passwordLower.Contains(info));
    }

    /// <summary>
    /// Verifica se os endereços são válidos
    /// </summary>
    private static bool HaveValidAddresses(List<CreateAddressDto> addresses)
    {
        if (addresses == null || !addresses.Any())
            return false;

        return addresses.All(address => !string.IsNullOrWhiteSpace(address.Street) && 
                                      !string.IsNullOrWhiteSpace(address.City) &&
                                      !string.IsNullOrWhiteSpace(address.State) &&
                                      !string.IsNullOrWhiteSpace(address.ZipCode));
    }

    /// <summary>
    /// Verifica se há apenas um endereço principal
    /// </summary>
    private static bool HaveOnlyOnePrimary(List<CreateAddressDto> addresses)
    {
        return addresses?.Count(a => a.IsPrimary) <= 1;

    }

    /// <summary>
    /// Obtém erros de validação da senha
    /// </summary>
    private List<string> GetPasswordValidationErrors(string password)
    {
        return _passwordService.GetPasswordValidationErrors(password);
    }

    // Validador inline para endereços
    private class AddressValidator : AbstractValidator<CreateAddressDto>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Rua é obrigatória")
                .MinimumLength(5).WithMessage("Rua deve ter no mínimo 5 caracteres")
                .MaximumLength(200).WithMessage("Rua deve ter no máximo 200 caracteres");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Cidade é obrigatória")
                .MinimumLength(2).WithMessage("Cidade deve ter no mínimo 2 caracteres")
                .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Cidade deve conter apenas letras e espaços");

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("Estado é obrigatório")
                .Length(2).WithMessage("Estado deve ter 2 caracteres")
                .Matches(@"^[A-Z]{2}$").WithMessage("Estado deve ser a sigla em maiúsculas");

            RuleFor(x => x.ZipCode)
                .NotEmpty().WithMessage("CEP é obrigatório")
                .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP inválido");

            RuleFor(x => x.Country)
                .MaximumLength(100).WithMessage("País deve ter no máximo 100 caracteres");
        }
    }
}

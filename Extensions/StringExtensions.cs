using System.Text.RegularExpressions;

namespace CadPlus.Extensions;

/// <summary>
/// Extensões úteis para manipulação de strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Remove formatação do CPF (pontos, traços, espaços)
    /// </summary>
    /// <param name="cpf">CPF com ou sem formatação</param>
    /// <returns>CPF apenas com números</returns>
    public static string CleanCpf(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return string.Empty;

        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Remove formatação do telefone
    /// </summary>
    /// <param name="phone">Telefone com formatação</param>
    /// <returns>Telefone apenas com números</returns>
    public static string CleanPhone(this string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return new string(phone.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Remove formatação do CEP
    /// </summary>
    /// <param name="zipCode">CEP com formatação</param>
    /// <returns>CEP apenas com números</returns>
    public static string CleanZipCode(this string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            return string.Empty;

        return new string(zipCode.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Normaliza email (trim + tolower)
    /// </summary>
    /// <param name="email">Email para normalizar</param>
    /// <returns>Email normalizado</returns>
    public static string NormalizeEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Verifica se string contém apenas letras e espaços
    /// </summary>
    /// <param name="text">Texto para validar</param>
    /// <returns>True se válido</returns>
    public static bool ContainsOnlyLettersAndSpaces(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return Regex.IsMatch(text, @"^[a-zA-ZÀ-ÿ\s]+$");
    }

    /// <summary>
    /// Verifica se é um CPF válido (formato)
    /// </summary>
    /// <param name="cpf">CPF para validar</param>
    /// <returns>True se formato válido</returns>
    public static bool IsValidCpfFormat(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var cleanCpf = cpf.CleanCpf();
        return cleanCpf.Length == 11 && cleanCpf.All(char.IsDigit);
    }

    /// <summary>
    /// Verifica se é um telefone brasileiro válido
    /// </summary>
    /// <param name="phone">Telefone para validar</param>
    /// <returns>True se formato válido</returns>
    public static bool IsValidBrazilianPhone(this string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var cleanPhone = phone.CleanPhone();
        return cleanPhone.Length == 10 || cleanPhone.Length == 11;
    }

    /// <summary>
    /// Verifica se é um CEP brasileiro válido
    /// </summary>
    /// <param name="zipCode">CEP para validar</param>
    /// <returns>True se formato válido</returns>
    public static bool IsValidBrazilianZipCode(this string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            return false;

        var cleanZipCode = zipCode.CleanZipCode();
        return cleanZipCode.Length == 8;
    }

    /// <summary>
    /// Verifica se email tem formato válido
    /// </summary>
    /// <param name="email">Email para validar</param>
    /// <returns>True se formato válido</returns>
    public static bool IsValidEmailFormat(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.NormalizeEmail();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica se senha atende critérios mínimos de segurança
    /// </summary>
    /// <param name="password">Senha para validar</param>
    /// <returns>Lista de critérios não atendidos</returns>
    public static List<string> GetPasswordValidationErrors(this string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Senha é obrigatória");
            return errors;
        }

        if (password.Length < 8)
            errors.Add("Senha deve ter no mínimo 8 caracteres");

        if (password.Length > 128)
            errors.Add("Senha deve ter no máximo 128 caracteres");

        if (!password.Any(char.IsLower))
            errors.Add("Senha deve conter pelo menos uma letra minúscula");

        if (!password.Any(char.IsUpper))
            errors.Add("Senha deve conter pelo menos uma letra maiúscula");

        if (!password.Any(char.IsDigit))
            errors.Add("Senha deve conter pelo menos um número");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Senha deve conter pelo menos um caractere especial");

        // Verifica sequências comuns
        var commonSequences = new[] { "123", "abc", "qwe", "asd", "password", "senha", "123456", "12345678" };
        foreach (var sequence in commonSequences)
        {
            if (password.ToLower().Contains(sequence.ToLower()))
                errors.Add($"Senha não pode conter '{sequence}'");
        }

        return errors;
    }

    /// <summary>
    /// Remove caracteres especiais e converte para formato seguro para URL/slug
    /// </summary>
    /// <param name="text">Texto para limpar</param>
    /// <returns>Texto limpo</returns>
    public static string ToUrlSafe(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove acentos
        var normalizedText = text.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();

        foreach (var c in normalizedText)
        {
            if (char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var result = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

        // Remove caracteres especiais, mantém apenas letras, números e espaços
        result = Regex.Replace(result, @"[^a-zA-Z0-9\s-]", string.Empty);

        // Substitui espaços por hífens e remove múltiplos hífens
        result = Regex.Replace(result, @"\s+", "-");
        result = Regex.Replace(result, @"-[-]+", "-");

        return result.ToLowerInvariant().Trim('-');
    }

    /// <summary>
    /// Trunca string no tamanho especificado e adiciona elipsis se necessário
    /// </summary>
    /// <param name="text">Texto para truncar</param>
    /// <param name="maxLength">Tamanho máximo</param>
    /// <param name="ellipsis">Texto para elipsis (padrão: "...")</param>
    /// <returns>Texto truncado</returns>
    public static string Truncate(this string text, int maxLength, string ellipsis = "...")
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
            return text;

        return text[..(maxLength - ellipsis.Length)] + ellipsis;
    }

    /// <summary>
    /// Verifica se string é nula, não definida ou apenas espaços em branco
    /// </summary>
    /// <param name="value">String para verificar</param>
    /// <returns>True se vazia</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Verifica se string não é nula, não definida ou apenas espaços em branco
    /// </summary>
    /// <param name="value">String para verificar</param>
    /// <returns>True se não vazia</returns>
    public static bool IsNotNullOrWhiteSpace(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Capitaliza primeira letra
    /// </summary>
    /// <param name="text">texto para capitalizar</param>
    /// <returns>Texto com primeira letra maiúscula</returns>
    public static string CapitalizeFirst(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return char.ToUpper(text[0]) + (text.Length > 1 ? text[1..].ToLowerInvariant() : string.Empty);
    }

    /// <summary>
    /// Capitaliza cada palavra
    /// </summary>
    /// <param name="text">Texto para capitalizar</param>
    /// <returns>Texto com cada palavra capitalizada</returns>
    public static string CapitalizeWords(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Select(CapitalizeFirst));
    }
}

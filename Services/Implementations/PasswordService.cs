using CadPlus.Services;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace CadPlus.Services.Implementations;

/// <summary>
/// Implementação do serviço de senha usando BCrypt
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password não pode ser vazio", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public bool IsStrongPassword(string password)
    {
        var errors = GetPasswordValidationErrors(password);
        return errors.Count == 0;
    }

    public List<string> GetPasswordValidationErrors(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Senha é obrigatória");
            return errors;
        }

        // Comprimento mínimo
        if (password.Length < 8)
            errors.Add("Senha deve ter no mínimo 8 caracteres");

        // Máximo de 128 caracteres (limite de segurança)
        if (password.Length > 128)
            errors.Add("Senha deve ter no máximo 128 caracteres");

        // Pelo menos uma letra minúscula
        if (!password.Any(char.IsLower))
            errors.Add("Senha deve conter pelo menos uma letra minúscula");

        // Pelo menos uma letra maiúscula
        if (!password.Any(char.IsUpper))
            errors.Add("Senha deve conter pelo menos uma letra maiúscula");

        // Pelo menos um número
        if (!password.Any(char.IsDigit))
            errors.Add("Senha deve conter pelo menos um número");

        // Pelo menos um caractere especial
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Senha deve conter pelo menos um caractere especial (!@#$%^&* etc.)");

        // Verifica sequências comuns
        var commonSequences = new[] { "123", "abc", "qwe", "asd", "zxcv" };
        foreach (var sequence in commonSequences)
        {
            if (password.ToLower().Contains(sequence.ToLower()))
                errors.Add($"Senha não pode conter sequências comuns como '{sequence}'");
        }

        // Verifica senhas muito simples
        if (IsSimplePassword(password))
            errors.Add("Senha muito simples. Use uma combinação mais complexa");

        return errors;
    }

    /// <summary>
    /// Verifica se a senha é muito simples
    /// </summary>
    /// <param name="password">Senha a ser verificada</param>
    /// <returns>True se muito simples</returns>
    private static bool IsSimplePassword(string password)
    {
        // Padrões de senhas muito simples
        var simplePatterns = new[]
        {
            @"^.{8}$",  // Exatamente 8 caracteres iguais
            @"^(.)\1{7,}$",  // Todos os caracteres iguais
            @"^(.){8}$",  // Todos caracteres diferentes mas simples
            @"^[0-9]{8}$",  // Apenas números
            @"^[a-z]{8}$",  // Apenas letras minúsculas
            @"^[A-Z]{8}$"   // Apenas letras maiúsculas
        };

        foreach (var pattern in simplePatterns)
        {
            if (Regex.IsMatch(password, pattern))
                return true;
        }

        // Verifica se é uma palavra comum (implementação básica)
        var commonWords = new[] { "password", "12345678", "qwertyui", "abcdefgh" };
        if (commonWords.Contains(password.ToLower()))
            return true;

        return false;
    }
}

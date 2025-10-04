using CadPlus.Services;

namespace CadPlus.Services.Implementations;

/// <summary>
/// Implementação do serviço de validação de CPF
/// </summary>
public class CpfValidationService : ICpfValidationService
{
    private static readonly int[] _firstVerifier = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] _secondVerifier = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

    public bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove formatação
        var cleanCpf = Clean(cpf);
        
        // Verifica se tem 11 dígitos
        if (cleanCpf.Length != 11)
            return false;

        // Verifica se são apenas números
        if (!cleanCpf.All(char.IsDigit))
            return false;

        // Verifica sequências inválidas (111.111.111-11, etc)
        if (cleanCpf.All(c => c == cleanCpf[0]))
            return false;

        // Converte para array de números
        var numbers = cleanCpf.Select(c => int.Parse(c.ToString())).ToArray();
        
        // Valida primeiro dígito verificador
        var firstSum = numbers.Take(9).Zip(_firstVerifier, (number, verifier) => number * verifier).Sum();
        var firstDigit = firstSum % 11 < 2 ? 0 : 11 - (firstSum % 11);
        
        if (numbers[9] != firstDigit)
            return false;

        // Valida segundo dígito verificador
        var secondSum = numbers.Take(10).Zip(_secondVerifier, (number, verifier) => number * verifier).Sum();
        var secondDigit = secondSum % 11 < 2 ? 0 : 11 - (secondSum % 11);
        
        return numbers[10] == secondDigit;
    }

    public string Format(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf) || !IsValid(cpf))
            return cpf;

        var cleanCpf = Clean(cpf);
        return $"{cleanCpf[..3]}.{cleanCpf[3..6]}.{cleanCpf[6..9]}-{cleanCpf[9..]}";
    }

    public string Clean(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return string.Empty;

        // Remove todos os caracteres não numéricos
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    public string GenerateValidCpf()
    {
        var random = new Random();
        
        // Gera 9 dígitos aleatórios
        var numbers = new int[9];
        for (int i = 0; i < 9; i++)
        {
            numbers[i] = random.Next(0, 10);
        }
        
        // Calcula primeiro dígito verificador
        var firstSum = numbers.Zip(_firstVerifier, (number, verifier) => number * verifier).Sum();
        var firstDigit = firstSum % 11 < 2 ? 0 : 11 - (firstSum % 11);
        
        // Calcula segundo dígito verificador
        var secondSum = numbers.Append(firstDigit).Zip(_secondVerifier, (number, verifier) => number * verifier).Sum();
        var secondDigit = secondSum % 11 < 2 ? 0 : 11 - (secondSum % 11);
        
        // Retorna CPF completo
        var cpfNumbers = numbers.Append(firstDigit).Append(secondDigit).ToArray();
        return new string(cpfNumbers.Select(n => n.ToString()[0]).ToArray());
    }
}

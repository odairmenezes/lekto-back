namespace CadPlus.Services;

/// <summary>
/// Serviço responsável pela validação de CPF brasileiro
/// </summary>
public interface ICpfValidationService
{
    /// <summary>
    /// Valida se o CPF possui formato e algoritmo correto
    /// </summary>
    /// <param name="cpf"> CPF no formato 11 dígitos</param>
    /// <returns>True se válido, false caso contrário</returns>
    bool IsValid(string cpf);
    
    /// <summary>
    /// Formata o CPF com pontos e traço (XXX.XXX.XXX-XX)
    /// </summary>
    /// <param name="cpf">CPF sem formatação</param>
    /// <returns>CPF formatado</returns>
    string Format(string cpf);
    
    /// <summary>
    /// Remove formatação do CPF, deixando apenas números
    /// </summary>
    /// <param name="cpf">CPF com ou sem formatação</param>
    /// <returns>CPF apenas com números</returns>
    string Clean(string cpf);
    
    /// <summary>
    /// Gera um CPF válido para testes
    /// </summary>
    /// <returns>CPF válido</returns>
    string GenerateValidCpf();
}

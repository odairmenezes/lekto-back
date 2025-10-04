namespace CadPlus.Services;

/// <summary>
/// Serviço responsável por operações de senha
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Cria hash da senha usando BCrypt
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash da senha</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifica se a senha confere com o hash
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <param name="hash">Hash armazenado</param>
    /// <returns>True se confere, false caso contrário</returns>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Valida se a senha atende aos critérios de segurança
    /// </summary>
    /// <param name="password">Senha a ser validada</param>
    /// <returns>True se válida, false caso contrário</returns>
    bool IsStrongPassword(string password);
    
    /// <summary>
    /// Retorna os critérios que a senha deve atender
    /// </summary>
    /// <param name="password">Senha a ser validada</param>
    /// <returns>Lista de critérios não atendidos</returns>
    List<string> GetPasswordValidationErrors(string password);
}

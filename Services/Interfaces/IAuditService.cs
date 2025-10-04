using CadPlus.Models;

namespace CadPlus.Services.Interfaces;

/// <summary>
/// Interface para serviços de auditoria
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra uma alteração em uma entidade
    /// </summary>
    /// <param name="userId">ID do usuário que fez a alteração</param>
    /// <param name="entityType">Tipo da entidade (User, Address, etc.)</param>
    /// <param name="entityId">ID da entidade alterada</param>
    /// <param name="fieldName">Nome do campo alterado</param>
    /// <param name="oldValue">Valor anterior</param>
    /// <param name="newValue">Novo valor</param>
    /// <param name="ipAddress">Endereço IP da requisição</param>
    /// <param name="userAgent">User Agent da requisição</param>
    Task LogChangeAsync(Guid userId, string entityType, Guid entityId, string fieldName, 
        string? oldValue, string? newValue, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Registra múltiplas alterações em uma entidade
    /// </summary>
    /// <param name="userId">ID do usuário que fez a alteração</param>
    /// <param name="entityType">Tipo da entidade</param>
    /// <param name="entityId">ID da entidade alterada</param>
    /// <param name="changes">Dicionário com as alterações (campo -> {oldValue, newValue})</param>
    /// <param name="ipAddress">Endereço IP da requisição</param>
    /// <param name="userAgent">User Agent da requisição</param>
    Task LogChangesAsync(Guid userId, string entityType, Guid entityId, 
        Dictionary<string, (string? oldValue, string? newValue)> changes, 
        string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Busca logs de auditoria por usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="page">Página</param>
    /// <param name="limit">Limite por página</param>
    Task<(List<AuditLog> logs, int totalCount)> GetLogsByUserAsync(Guid userId, int page = 1, int limit = 20);

    /// <summary>
    /// Busca logs de auditoria por entidade
    /// </summary>
    /// <param name="entityType">Tipo da entidade</param>
    /// <param name="entityId">ID da entidade</param>
    /// <param name="page">Página</param>
    /// <param name="limit">Limite por página</param>
    Task<(List<AuditLog> logs, int totalCount)> GetLogsByEntityAsync(string entityType, Guid entityId, int page = 1, int limit = 20);

    /// <summary>
    /// Busca logs de auditoria por período
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="page">Página</param>
    /// <param name="limit">Limite por página</param>
    Task<(List<AuditLog> logs, int totalCount)> GetLogsByPeriodAsync(DateTime startDate, DateTime endDate, int page = 1, int limit = 20);

    /// <summary>
    /// Busca logs de auditoria por ação
    /// </summary>
    /// <param name="fieldName">Nome do campo alterado</param>
    /// <param name="page">Página</param>
    /// <param name="limit">Limite por página</param>
    Task<(List<AuditLog> logs, int totalCount)> GetLogsByActionAsync(string fieldName, int page = 1, int limit = 20);
}

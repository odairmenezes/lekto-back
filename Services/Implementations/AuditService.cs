using CadPlus.Data;
using CadPlus.Models;
using CadPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CadPlus.Services.Implementations;

/// <summary>
/// Serviço para gerenciar logs de auditoria
/// </summary>
public class AuditService : IAuditService
{
    private readonly CadPlusDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(CadPlusDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Registra uma alteração em uma entidade
    /// </summary>
    public async Task LogChangeAsync(Guid userId, string entityType, Guid entityId, string fieldName, 
        string? oldValue, string? newValue, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            // Só registra se houve mudança real (mas permite criação com oldValue null)
            if (oldValue != null && oldValue == newValue)
                return;

            var auditLog = new AuditLog
            {
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Audit log created: {EntityType}.{FieldName} changed from '{OldValue}' to '{NewValue}' by user {UserId}", 
                entityType, fieldName, oldValue, newValue, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for {EntityType}.{FieldName} by user {UserId}", 
                entityType, fieldName, userId);
        }
    }

    /// <summary>
    /// Registra múltiplas alterações em uma entidade
    /// </summary>
    public async Task LogChangesAsync(Guid userId, string entityType, Guid entityId, 
        Dictionary<string, (string? oldValue, string? newValue)> changes, 
        string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var auditLogs = new List<AuditLog>();
            var timestamp = DateTime.UtcNow;

            foreach (var change in changes)
            {
                // Só registra se houve mudança real
                if (change.Value.oldValue == change.Value.newValue)
                    continue;

                auditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    EntityType = entityType,
                    EntityId = entityId,
                    FieldName = change.Key,
                    OldValue = change.Value.oldValue,
                    NewValue = change.Value.newValue,
                    ChangedAt = timestamp,
                    ChangedBy = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }

            if (auditLogs.Any())
            {
                _context.AuditLogs.AddRange(auditLogs);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Audit logs created: {Count} changes for {EntityType} by user {UserId}", 
                    auditLogs.Count, entityType, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit logs for {EntityType} by user {UserId}", 
                entityType, userId);
        }
    }

    /// <summary>
    /// Busca logs de auditoria por CPF do usuário
    /// </summary>
    public async Task<(List<AuditLog> logs, int totalCount)> GetLogsByCpfAsync(string cpf, int page = 1, int limit = 20)
    {
        _logger.LogInformation("Buscando logs de auditoria por CPF: {Cpf}", cpf);

        var query = _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.User.Cpf == cpf);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.ChangedAt)
            .ThenBy(a => a.FieldName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (logs, totalCount);
    }
}
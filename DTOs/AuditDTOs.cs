using System.ComponentModel.DataAnnotations;

namespace CadPlus.DTOs;

/// <summary>
/// DTO para resposta de log de auditoria
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid ChangedBy { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO para busca de logs de auditoria
/// </summary>
public class AuditLogSearchDto
{
    public Guid? UserId { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? FieldName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

/// <summary>
/// DTO para resposta paginada de logs de auditoria
/// </summary>
public class AuditLogResponseDto
{
    public List<AuditLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int ItemsPerPage { get; set; }
}

/// <summary>
/// DTO para estatísticas de auditoria
/// </summary>
public class AuditStatsDto
{
    public int TotalLogs { get; set; }
    public int LogsToday { get; set; }
    public int LogsThisWeek { get; set; }
    public int LogsThisMonth { get; set; }
    public Dictionary<string, int> LogsByEntityType { get; set; } = new();
    public Dictionary<string, int> LogsByField { get; set; } = new();
    public Dictionary<Guid, int> LogsByUser { get; set; } = new();
}

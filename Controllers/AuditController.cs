using CadPlus.DTOs;
using CadPlus.Models;
using CadPlus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CadPlus.Controllers;

/// <summary>
/// Controller para gerenciar logs de auditoria
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Busca logs de auditoria por usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="limit">Limite por página (padrão: 20)</param>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<AuditLogResponseDto>>> GetLogsByUser(
        Guid userId, 
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var (logs, totalCount) = await _auditService.GetLogsByUserAsync(userId, page, limit);
            
            var response = new AuditLogResponseDto
            {
                Logs = logs.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit),
                ItemsPerPage = limit
            };

            return Ok(new ApiResponse<AuditLogResponseDto>
            {
                Success = true,
                Message = "Logs de auditoria recuperados com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Busca logs de auditoria por entidade
    /// </summary>
    /// <param name="entityType">Tipo da entidade</param>
    /// <param name="entityId">ID da entidade</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="limit">Limite por página (padrão: 20)</param>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<ApiResponse<AuditLogResponseDto>>> GetLogsByEntity(
        string entityType, 
        Guid entityId, 
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var (logs, totalCount) = await _auditService.GetLogsByEntityAsync(entityType, entityId, page, limit);
            
            var response = new AuditLogResponseDto
            {
                Logs = logs.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit),
                ItemsPerPage = limit
            };

            return Ok(new ApiResponse<AuditLogResponseDto>
            {
                Success = true,
                Message = "Logs de auditoria recuperados com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for entity {EntityType}/{EntityId}", entityType, entityId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Busca logs de auditoria por período
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="limit">Limite por página (padrão: 20)</param>
    [HttpGet("period")]
    public async Task<ActionResult<ApiResponse<AuditLogResponseDto>>> GetLogsByPeriod(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;
            if (startDate > endDate)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data inicial não pode ser maior que a data final",
                    Errors = new List<string> { "Período inválido" }
                });

            var (logs, totalCount) = await _auditService.GetLogsByPeriodAsync(startDate, endDate, page, limit);
            
            var response = new AuditLogResponseDto
            {
                Logs = logs.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit),
                ItemsPerPage = limit
            };

            return Ok(new ApiResponse<AuditLogResponseDto>
            {
                Success = true,
                Message = "Logs de auditoria recuperados com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for period {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Busca logs de auditoria por ação
    /// </summary>
    /// <param name="fieldName">Nome do campo alterado</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="limit">Limite por página (padrão: 20)</param>
    [HttpGet("action/{fieldName}")]
    public async Task<ActionResult<ApiResponse<AuditLogResponseDto>>> GetLogsByAction(
        string fieldName, 
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var (logs, totalCount) = await _auditService.GetLogsByActionAsync(fieldName, page, limit);
            
            var response = new AuditLogResponseDto
            {
                Logs = logs.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit),
                ItemsPerPage = limit
            };

            return Ok(new ApiResponse<AuditLogResponseDto>
            {
                Success = true,
                Message = "Logs de auditoria recuperados com sucesso",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for action {FieldName}", fieldName);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Busca logs de auditoria com filtros múltiplos
    /// </summary>
    /// <param name="search">Parâmetros de busca</param>
    [HttpPost("search")]
    public ActionResult<ApiResponse<AuditLogResponseDto>> SearchLogs([FromBody] AuditLogSearchDto search)
    {
        try
        {
            if (search.Page < 1) search.Page = 1;
            if (search.Limit < 1 || search.Limit > 100) search.Limit = 20;

            // Implementar busca com filtros múltiplos
            // Por enquanto, retornar erro indicando que precisa ser implementado
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Busca com filtros múltiplos ainda não implementada",
                Errors = new List<string> { "Use os endpoints específicos para busca" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching audit logs");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Mapeia AuditLog para AuditLogDto
    /// </summary>
    private static AuditLogDto MapToDto(AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            FieldName = auditLog.FieldName,
            OldValue = auditLog.OldValue,
            NewValue = auditLog.NewValue,
            ChangedAt = auditLog.ChangedAt,
            ChangedBy = auditLog.ChangedBy,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            Description = null // Description não existe no modelo atual
        };
    }
}

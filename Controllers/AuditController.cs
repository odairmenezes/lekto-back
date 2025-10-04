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
    /// Busca logs de auditoria por CPF do usuário
    /// </summary>
    /// <param name="cpf">CPF do usuário</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="limit">Limite por página (padrão: 20)</param>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuditLogResponseDto>>> GetLogsByCpf(
        string cpf, 
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "CPF é obrigatório",
                    Errors = new List<string> { "CPF não pode ser vazio" }
                });

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var (logs, totalCount) = await _auditService.GetLogsByCpfAsync(cpf, page, limit);
            
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
            _logger.LogError(ex, "Error getting audit logs for CPF {Cpf}", cpf);
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
            UserAgent = auditLog.UserAgent
        };
    }
}
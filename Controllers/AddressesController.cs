using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FluentValidation;
using CadPlus.Data;
using CadPlus.DTOs;
using CadPlus.Models;
using CadPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CadPlus.Controllers;

/// <summary>
/// Controller responsável pelas operações de endereços dos usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class AddressesController : ControllerBase
{
    private readonly CadPlusDbContext _context;
    private readonly ILogger<AddressesController> _logger;
    private readonly IValidator<CreateAddressDto> _createAddressValidator;
    private readonly IValidator<UpdateAddressDto> _updateAddressValidator;
    private readonly IAuditService _auditService;

    public AddressesController(
        CadPlusDbContext context, 
        ILogger<AddressesController> logger,
        IValidator<CreateAddressDto> createAddressValidator,
        IValidator<UpdateAddressDto> updateAddressValidator,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _createAddressValidator = createAddressValidator;
        _updateAddressValidator = updateAddressValidator;
        _auditService = auditService;
    }

    /// <summary>
    /// Listar endereços de um usuário específico
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de endereços do usuário</returns>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<List<AddressDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<AddressDto>>>> GetUserAddresses(Guid userId)
    {
        try
        {
            _logger.LogInformation("Buscando endereços do usuário - UserId: {UserId}", userId);

            // Verificar se usuário existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                _logger.LogWarning("Usuário não encontrado para listagem de endereços - UserId: {UserId}", userId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não encontrado",
                    Errors = new List<string> { $"Usuário com ID {userId} não foi encontrado" }
                });
            }

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsPrimary) // Principal primeiro
                .ThenBy(a => a.CreatedAt)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    Street = a.Street,
                    Number = a.Number,
                    Neighborhood = a.Neighborhood,
                    Complement = a.Complement,
                    City = a.City,
                    State = a.State,
                    ZipCode = a.ZipCode,
                    Country = a.Country,
                    IsPrimary = a.IsPrimary
                })
                .ToListAsync();

            _logger.LogInformation("Endereços listados com sucesso - UserId: {UserId}, Total: {Count}", userId, addresses.Count);

            return Ok(new ApiResponse<List<AddressDto>>
            {
                Success = true,
                Message = "Endereços listados com sucesso",
                Data = addresses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante listagem de endereços - UserId: {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Buscar endereço por ID
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <returns>Dados do endereço</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AddressDto>>> GetAddress(Guid id)
    {
        try
        {
            _logger.LogInformation("Buscando endereço por ID: {AddressId}", id);

            var address = await _context.Addresses
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (address == null)
            {
                _logger.LogWarning("Endereço não encontrado - ID: {AddressId}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Endereço não encontrado",
                    Errors = new List<string> { $"Endereço com ID {id} não foi encontrado" }
                });
            }

            var addressDto = new AddressDto
            {
                Id = address.Id,
                Street = address.Street,
                Number = address.Number,
                Neighborhood = address.Neighborhood,
                Complement = address.Complement,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Country = address.Country,
                IsPrimary = address.IsPrimary
            };

            _logger.LogInformation("Endereço encontrado com sucesso - ID: {AddressId}", id);

            return Ok(new ApiResponse<AddressDto>
            {
                Success = true,
                Message = "Endereço encontrado com sucesso",
                Data = addressDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante busca de endereço - ID: {AddressId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Adicionar novo endereço para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="createAddressDto">Dados do endereço</param>
    /// <returns>Endereço criado</returns>
    [HttpPost("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AddressDto>>> CreateAddress(Guid userId, [FromBody] CreateAddressDto createAddressDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var requesterUserId = HttpContext.User.FindFirst("sub")?.Value;
        
        using var logScope = _logger.BeginScope("RequestId: {RequestId}, Action: CreateAddress, UserId: {UserId}, RequestedBy: {RequesterUserId}", requestId, userId, requesterUserId);
        
        try
        {
            _logger.LogInformation("BeginCreateAddress - Criando endereço para usuário - UserId: {UserId}, RequestedBy: {RequesterUserId}, City: {City}, State: {State}, UserAgent: {UserAgent}, IP: {ClientIP}", 
                userId, requesterUserId, createAddressDto.City, createAddressDto.State,
                HttpContext.Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Verificar se usuário existe e está ativo
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            if (user == null)
            {
                _logger.LogWarning("CreateAddressUserNotFound - Usuário não encontrado para criação de endereço - UserId: {UserId}, Duration: {Duration}ms", 
                    userId, stopwatch.ElapsedMilliseconds);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não encontrado ou inativo",
                    Errors = new List<string> { $"Usuário com ID {userId} não foi encontrado" }
                });
            }

            _logger.LogDebug("CreateAddressUserFound - Usuário encontrado e ativo - UserId: {UserId}", userId);

            // Validar se criaria endereço duplicado
            if (await WouldCreateDuplicateAddress(userId, createAddressDto))
            {
                stopwatch.Stop();
                _logger.LogWarning("CreateAddressDuplicate - Tentativa de criar endereço duplicado - UserId: {UserId}, Duration: {Duration}ms", 
                    userId, stopwatch.ElapsedMilliseconds);
                
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Endereço duplicado",
                    Errors = new List<string> { "Já existe um endereço idêntico para este usuário" }
                });
            }

            // Se marcado como principal, desmarcar outros endereços principais
            if (createAddressDto.IsPrimary)
            {
                _logger.LogDebug("CreateAddressDesactivateOthers - Desativando outros endereços principais para UserId: {UserId}", userId);
                await DesactivatePrimaryAddresses(userId);
            }

            // Criar novo endereço com todos os campos
            var address = new Address
            {
                UserId = userId,
                Street = createAddressDto.Street.Trim(),
                Number = !string.IsNullOrWhiteSpace(createAddressDto.Number) ? createAddressDto.Number.Trim() : null,
                Neighborhood = !string.IsNullOrWhiteSpace(createAddressDto.Neighborhood) ? createAddressDto.Neighborhood.Trim() : null,
                Complement = !string.IsNullOrWhiteSpace(createAddressDto.Complement) ? createAddressDto.Complement.Trim() : null,
                City = createAddressDto.City.Trim(),
                State = createAddressDto.State.Trim().ToUpperInvariant(),
                ZipCode = createAddressDto.ZipCode.Trim(),
                Country = !string.IsNullOrWhiteSpace(createAddressDto.Country) ? createAddressDto.Country.Trim() : "Brasil",
                IsPrimary = createAddressDto.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Registrar log de auditoria para criação de endereço
            await _auditService.LogChangeAsync(
                userId,
                "Address",
                address.Id,
                "Created",
                null,
                $"Endereço criado: {address.Street}, {address.Number}, {address.City}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            var addressDto = new AddressDto
            {
                Id = address.Id,
                Street = address.Street,
                Number = address.Number,
                Neighborhood = address.Neighborhood,
                Complement = address.Complement,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Country = address.Country,
                IsPrimary = address.IsPrimary
            };

            stopwatch.Stop();
            _logger.LogInformation("CreateAddressSuccess - Endereço criado com sucesso - UserId: {UserId}, AddressId: {AddressId}, IsPrimary: {IsPrimary}, City: {City}, State: {State}, RequestedBy: {RequesterUserId}, Duration: {Duration}ms", 
                userId, address.Id, address.IsPrimary, address.City, address.State, requesterUserId, stopwatch.ElapsedMilliseconds);

            return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, new ApiResponse<AddressDto>
            {
                Success = true,
                Message = "Endereço criado com sucesso",
                Data = addressDto
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CreateAddressError - Erro durante criação de endereço - UserId: {UserId}, Duration: {Duration}ms", 
                userId, stopwatch.ElapsedMilliseconds);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Atualizar endereço existente
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <param name="updateAddressDto">Dados para atualização</param>
    /// <returns>Endereço atualizado</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AddressDto>>> UpdateAddress(Guid id, [FromBody] UpdateAddressDto updateAddressDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        try
        {
            _logger.LogInformation("BeginUpdateAddress - Atualizando endereço - ID: {AddressId}, RequestId: {RequestId}", id, requestId);

            // Validar dados de entrada
            var validationResult = await _updateAddressValidator.ValidateAsync(updateAddressDto);
            if (!validationResult.IsValid)
            {
                stopwatch.Stop();
                _logger.LogWarning("UpdateAddressValidationFailed - Dados inválidos para endereço ID: {AddressId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var address = await _context.Addresses
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (address == null)
            {
                stopwatch.Stop();
                _logger.LogWarning("UpdateAddressNotFound - Endereço não encontrado - ID: {AddressId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Endereço não encontrado",
                    Errors = new List<string> { $"Endereço com ID {id} não foi encontrado" }
                });
            }

            _logger.LogDebug("UpdateAddressValidation - Endereço encontrado, iniciando validação de duplicatas - ID: {AddressId}", id);

            // Validar se criaria endereço duplicado para o mesmo usuário
            if (await WouldCreateDuplicateAddressAsync(address.UserId, updateAddressDto, id))
            {
                stopwatch.Stop();
                _logger.LogWarning("UpdateAddressDuplicate - Tentativa de criar endereço duplicado - UserId: {UserId}, Duration: {Duration}ms", 
                    address.UserId, stopwatch.ElapsedMilliseconds);
                
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Endereço duplicado",
                    Errors = new List<string> { "Já existe um endereço idêntico para este usuário" }
                });
            }

            // Se marcado como principal, desmarcar outros endereços principais do mesmo usuário
            if (updateAddressDto.IsPrimary == true)
            {
                await DesactivatePrimaryAddresses(address.UserId, id);
            }

            // Atualizar apenas campos fornecidos
            var hasChanges = false;

            if (!string.IsNullOrWhiteSpace(updateAddressDto.Street) && updateAddressDto.Street != address.Street)
            {
                address.Street = updateAddressDto.Street.Trim();
                hasChanges = true;
            }

            if (updateAddressDto.Number != null && updateAddressDto.Number != address.Number)
            {
                address.Number = !string.IsNullOrWhiteSpace(updateAddressDto.Number) ? 
                                updateAddressDto.Number.Trim()
                                : null;
                hasChanges = true;
            }

            if (updateAddressDto.Neighborhood != null && updateAddressDto.Neighborhood != address.Neighborhood)
            {
                address.Neighborhood = !string.IsNullOrWhiteSpace(updateAddressDto.Neighborhood) ? 
                                      updateAddressDto.Neighborhood.Trim()
                                      : null;
                hasChanges = true;
            }

            if (updateAddressDto.Complement != null && updateAddressDto.Complement != address.Complement)
            {
                address.Complement = !string.IsNullOrWhiteSpace(updateAddressDto.Complement) ? 
                                    updateAddressDto.Complement.Trim()
                                    : null;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(updateAddressDto.City) && updateAddressDto.City != address.City)
            {
                address.City = updateAddressDto.City.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(updateAddressDto.State) && updateAddressDto.State != address.State)
            {
                address.State = updateAddressDto.State.Trim().ToUpperInvariant();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(updateAddressDto.ZipCode) && updateAddressDto.ZipCode != address.ZipCode)
            {
                address.ZipCode = updateAddressDto.ZipCode.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(updateAddressDto.Country) && updateAddressDto.Country != address.Country)
            {
                address.Country = updateAddressDto.Country.Trim();
                hasChanges = true;
            }

            if (updateAddressDto.IsPrimary.HasValue && updateAddressDto.IsPrimary.Value != address.IsPrimary)
            {
                address.IsPrimary = updateAddressDto.IsPrimary.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
                
                // Registrar log de auditoria para atualização de endereço
                await _auditService.LogChangeAsync(
                    address.UserId,
                    "Address",
                    address.Id,
                    "Updated",
                    "Endereço anterior",
                    $"Endereço atualizado: {address.Street}, {address.Number}, {address.City}",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HttpContext.Request.Headers["User-Agent"].ToString()
                );
                
                stopwatch.Stop();
                _logger.LogInformation("UpdateAddressSuccess - Endereço atualizado com sucesso - ID: {AddressId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                stopwatch.Stop();
                _logger.LogInformation("UpdateAddressNoChanges<｜tool▁calls▁begin｜>Endereço não teve alterações - ID: {AddressId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
            }

            var addressDto = new AddressDto
            {
                Id = address.Id,
                Street = address.Street,
                Number = address.Number,
                Neighborhood = address.Neighborhood,
                Complement = address.Complement,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Country = address.Country,
                IsPrimary = address.IsPrimary
            };

            return Ok(new ApiResponse<AddressDto>
            {
                Success = true,
                Message = "Endereço atualizado com sucesso",
                Data = addressDto
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "UpdateAddressError - Erro durante atualização de endereço - ID: {AddressId}, Duration: {Duration}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Deletar endereço
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <returns>Status da remoção</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAddress(Guid id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        try
        {
            _logger.LogInformation("BeginDeleteAddress - Deletando endereço - ID: {AddressId}, RequestId: {RequestId}", id, requestId);

            var address = await _context.Addresses
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (address == null)
            {
                stopwatch.Stop();
                _logger.LogWarning("DeleteAddressNotFound - Endereço não encontrado - ID: {AddressId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Endereço não encontrado",
                    Errors = new List<string> { $"Endereço com ID {id} não foi encontrado" }
                });
            }

            // Verificar se é o único endereço do usuário
            var userAddressCount = await _context.Addresses
                .CountAsync(a => a.UserId == address.UserId);
                
            if (userAddressCount <= 1)
            {
                stopwatch.Stop();
                _logger.LogWarning("DeleteAddressLastAddress - Tentativa de deletar último endereço do usuário - UserId: {UserId}, Duration: {Duration}ms", 
                    address.UserId, stopwatch.ElapsedMilliseconds);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Não é possível deletar o último endereço",
                    Errors = new List<string> { "Cada usuário deve ter pelo menos um endereço" }
                });
            }

            _logger.LogDebug("DeleteAddressValidation - Aprovação para deletar endereço - ID: {AddressId}", id);

            // Remove o endereço
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            // Registrar log de auditoria para exclusão de endereço
            await _auditService.LogChangeAsync(
                address.UserId,
                "Address",
                address.Id,
                "Deleted",
                $"Endereço: {address.Street}, {address.Number}, {address.City}",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            stopwatch.Stop();
            _logger.LogInformation("DeleteAddressSuccess - Endereço deletado com sucesso - ID: {AddressId}, Duration: {Duration}ms", 
                id, stopwatch.ElapsedMilliseconds);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Endereço deletado com sucesso",
                Data = true
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DeleteAddressError - Erro durante exclusão de endereço - ID: {AddressId}, Duration: {Duration}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Definir endereço como principal
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <returns>Status da operação</returns>
    [HttpPost("{id}/set-primary")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> SetPrimaryAddress(Guid id)
    {
        try
        {
            _logger.LogInformation("Definindo endereço como principal - ID: {AddressId}", id);

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                _logger.LogWarning("Endereço não encontrado para definir como principal - ID: {AddressId}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Endereço não encontrado",
                    Errors = new List<string> { $"Endereço com ID {id} não foi encontrado" }
                });
            }

            // Desativar outros endereços principais do usuário
            await DesactivatePrimaryAddresses(address.UserId, id);

            // Definir este endereço como principal
            address.IsPrimary = true;
            await _context.SaveChangesAsync();

            // Registrar log de auditoria para definir endereço como principal
            await _auditService.LogChangeAsync(
                address.UserId,
                "Address",
                address.Id,
                "SetPrimary",
                "Endereço secundário",
                "Endereço definido como principal",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            _logger.LogInformation("Endereço definido como principal com sucesso - ID: {AddressId}", id);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Endereço definido como principal com sucesso",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir endereço como principal - ID: {AddressId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Desativa endereços principais de um usuário (exceto o especificado)
    /// </summary>
    /// <param name="userId">UserId</param>
    /// <param name="excludeId">AddressId para excluir</param>
    private async Task DesactivatePrimaryAddresses(Guid userId, Guid? excludeId = null)
    {
        var query = _context.Addresses.Where(a => a.UserId == userId && a.IsPrimary);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        var primaryAddresses = await query.ToListAsync();
        foreach (var addr in primaryAddresses)
        {
            addr.IsPrimary = false;
        }
    }

    /// <summary>
    /// Verifica se criaria endereço duplicado durante a criação
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="createAddressDto">Dados do endereço a ser criado</param>
    /// <returns>True se criaria duplicata</returns>
    private async Task<bool> WouldCreateDuplicateAddress(Guid userId, CreateAddressDto createAddressDto)
    {
        var street = createAddressDto.Street.Trim().ToLower();
        var number = !string.IsNullOrEmpty(createAddressDto.Number) ? createAddressDto.Number.Trim().ToLower() : string.Empty;
        var neighborhood = !string.IsNullOrEmpty(createAddressDto.Neighborhood) ? createAddressDto.Neighborhood.Trim().ToLower() : string.Empty;
        var city = createAddressDto.City.Trim().ToLower();
        var state = createAddressDto.State.Trim().ToUpper();
        var zipCode = createAddressDto.ZipCode.Trim();
        var country = createAddressDto.Country.Trim().ToLower();
        var complement = !string.IsNullOrEmpty(createAddressDto.Complement) ? createAddressDto.Complement.Trim().ToLower() : string.Empty;

        // Buscar endereços existentes do usuário
        var existingAddresses = await _context.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();

        // Comparar cada endereço existente
        foreach (var existingAddr in existingAddresses)
        {
            var existingStreet = existingAddr.Street?.Trim().ToLower() ?? string.Empty;
            var existingNumber = existingAddr.Number?.Trim().ToLower() ?? string.Empty;
            var existingNeighborhood = existingAddr.Neighborhood?.Trim().ToLower() ?? string.Empty;
            var existingCity = existingAddr.City?.Trim().ToLower() ?? string.Empty;
            var existingState = existingAddr.State?.Trim().ToUpper() ?? string.Empty;
            var existingZipCode = existingAddr.ZipCode?.Trim() ?? string.Empty;
            var existingCountry = existingAddr.Country?.Trim().ToLower() ?? string.Empty;
            var existingComplement = existingAddr.Complement?.Trim().ToLower() ?? string.Empty;

            // Verificar se todos os campos são iguais
            if (existingStreet == street &&
                existingNumber == number &&
                existingNeighborhood == neighborhood &&
                existingCity == city &&
                existingState == state &&
                existingZipCode == zipCode &&
                existingCountry == country &&
                existingComplement == complement)
            {
                return true; // Endereço duplicado encontrado
            }
        }

        return false; // Nenhum endereço duplicado encontrado
    }

    /// <summary>
    /// Verifica se criaria endereço duplicado durante a atualização
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="updateAddressDto">Dados do endereço a ser atualizado</param>
    /// <param name="currentAddressId">ID do endereço atual sendo atualizado</param>
    /// <returns>True se criaria duplicata</returns>
    private async Task<bool> WouldCreateDuplicateAddressAsync(Guid userId, UpdateAddressDto updateAddressDto, Guid currentAddressId)
    {
        // Normalizar dados para comparação
        var street = !string.IsNullOrEmpty(updateAddressDto.Street) ? updateAddressDto.Street.Trim().ToLower() : string.Empty;
        var number = !string.IsNullOrEmpty(updateAddressDto.Number) ? updateAddressDto.Number.Trim().ToLower() : string.Empty;
        var neighborhood = !string.IsNullOrEmpty(updateAddressDto.Neighborhood) ? updateAddressDto.Neighborhood.Trim().ToLower() : string.Empty;
        var city = !string.IsNullOrEmpty(updateAddressDto.City) ? updateAddressDto.City.Trim().ToLower() : string.Empty;
        var state = !string.IsNullOrEmpty(updateAddressDto.State) ? updateAddressDto.State.Trim().ToUpper() : string.Empty;
        var zipCode = !string.IsNullOrEmpty(updateAddressDto.ZipCode) ? updateAddressDto.ZipCode.Trim() : string.Empty;
        var country = !string.IsNullOrEmpty(updateAddressDto.Country) ? updateAddressDto.Country.Trim().ToLower() : string.Empty;
        var complement = !string.IsNullOrEmpty(updateAddressDto.Complement) ? updateAddressDto.Complement.Trim().ToLower() : string.Empty;

        var exists = await _context.Addresses.AnyAsync(a => 
            a.UserId == userId &&
            a.Id != currentAddressId &&
            a.Street.ToLower() == street &&
            ((a.Number ?? string.Empty).ToLower() == number) &&
            ((a.Neighborhood ?? string.Empty).ToLower() == neighborhood) &&
            a.City.ToLower() == city &&
            a.State.ToUpper() == state &&
            a.ZipCode == zipCode &&
            a.Country.ToLower() == country &&
            ((a.Complement == null && string.IsNullOrEmpty(complement)) ||
             (a.Complement != null && a.Complement.ToLower() == complement)));

        return exists;
    }
}

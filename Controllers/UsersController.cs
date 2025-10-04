using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FluentValidation;
using CadPlus.DTOs;
using CadPlus.Services;
using CadPlus.Data;
using Microsoft.EntityFrameworkCore;

namespace CadPlus.Controllers;

/// <summary>
/// Controller responsável pelas operações CRUD de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<UpdateUserDto> _updateUserValidator;
    private readonly ILogger<UsersController> _logger;
    private readonly CadPlusDbContext _context;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserDto> createUserValidator,
        IValidator<UpdateUserDto> updateUserValidator,
        ILogger<UsersController> logger,
        CadPlusDbContext context)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Listar usuários com paginação
    /// </summary>
    /// <param name="page">Número da página (opcional)</param>
    /// <param name="pageSize">Tamanho da página (opcional)</param>
    /// <param name="search">Termo de busca (opcional)</param>
    /// <returns>Lista paginada de usuários</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserDto>>>> GetUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var userId = HttpContext.User.FindFirst("sub")?.Value;
        
        using var logScope = _logger.BeginScope("RequestId: {RequestId}, Action: GetUsers, RequestedBy: {UserId}", requestId, userId);
        
        try
        {
            _logger.LogInformation("BeginGetUsers - Listagem de usuários solicitada - Página: {Page}, PageSize: {PageSize}, SearchTerm: {Search}, UserAgent: {UserAgent}, IP: {ClientIP}", 
                page, pageSize, search,
                HttpContext.Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Validar parâmetros de paginação
            if (page < 1)
            {
                _logger.LogWarning("GetUsersValidationFailed - Página inválida: {Page}, Duration: {Duration}ms", page, stopwatch.ElapsedMilliseconds);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Parâmetros inválidos",
                    Errors = new List<string> { "Página deve ser entre 1 e 10000" }
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _logger.LogWarning("GetUsersValidationFailed - PageSize inválido: {PageSize}, Duration: {Duration}ms", pageSize, stopwatch.ElapsedMilliseconds);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Parâmetros inválidos",
                    Errors = new List<string> { "Tamanho da página deve ser entre 1 e 100" }
                });
            }

            _logger.LogDebug("GetUsersValidation - Validação de parâmetros bem-sucedida, página: {Page}, pageSize: {PageSize}", page, pageSize);

            var result = await _userService.GetUsersAsync(page, pageSize, search);

            stopwatch.Stop();
            _logger.LogInformation("GetUsersSuccess - Listagem de usuários retornada com sucesso - Total: {TotalCount}, Página: {Page}, PageSize: {PageSize}, Resultados: {ItemsCount}, Duration: {Duration}ms", 
                result.TotalCount, page, pageSize, result.Data.Count, stopwatch.ElapsedMilliseconds);

            return Ok(new ApiResponse<PagedResponse<UserDto>>
            {
                Success = true,
                Message = "Usuários listados com sucesso",
                Data = result
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetUsersError - Erro durante listagem de usuários - Página: {Page}, PageSize: {PageSize}, Duration: {Duration}ms", 
                page, pageSize, stopwatch.ElapsedMilliseconds);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Obter usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Dados do usuário</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid id)
    {
        try
        {
            _logger.LogInformation("Busca de usuário solicitada - ID: {UserId}", id);

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado - ID: {UserId}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não encontrado",
                    Errors = new List<string> { $"Usuário com ID {id} não foi encontrado" }
                });
            }

            _logger.LogInformation("Usuário encontrado com sucesso - ID: {UserId}", id);

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuário encontrado com sucesso",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante busca de usuário - ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Criar novo usuário
    /// </summary>
    /// <param name="createUserDto">Dados do usuário</param>
    /// <returns>Usuário criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var requesterUserId = HttpContext.User.FindFirst("sub")?.Value;
        
        using var logScope = _logger.BeginScope("RequestId: {RequestId}, Action: CreateUser, RequestedBy: {RequesterUserId}", requestId, requesterUserId);
        
        try
        {
            _logger.LogInformation("BeginCreateUser - Criação de usuário solicitada - Email: {Email}, RequestedBy: {RequesterUserId}, UserAgent: {UserAgent}, IP: {ClientIP}", 
                createUserDto.Email, requesterUserId,
                HttpContext.Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Validar dados de entrada
            var validationResult = await _createUserValidator.ValidateAsync(createUserDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("CreateUserValidationFailed - Dados inválidos para e-mail: {Email}, Errors: {Errors}, Duration: {Duration}ms", 
                    createUserDto.Email, 
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    stopwatch.ElapsedMilliseconds);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            _logger.LogDebug("CreateUserValidation - Validação bem-sucedida para e-mail: {Email}", createUserDto.Email);

            // Criar usuário
            var createdUser = await _userService.CreateAsync(createUserDto);
            
            stopwatch.Stop();
            _logger.LogInformation("CreateUserSuccess - Usuário criado com sucesso - ID: {UserId}, Email: {Email}, RequestedBy: {RequesterUserId}, Duration: {Duration}ms", 
                createdUser.Id, createUserDto.Email, requesterUserId, stopwatch.ElapsedMilliseconds);
            
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuário criado com sucesso",
                Data = createdUser
            });
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("CreateUserConflict - Dados duplicados para e-mail: {Email}, Reason: {Reason}, Duration: {Duration}ms", 
                createUserDto.Email, ex.Message, stopwatch.ElapsedMilliseconds);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = "Dados já cadastrados",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CreateUserError - Erro durante criação de usuário - Email: {Email}, Duration: {Duration}ms", 
                createUserDto.Email, stopwatch.ElapsedMilliseconds);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// atualizar usuário existente
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="updateUserDto">Dados para atualização</param>
    /// <returns>Usuário atualizado</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            _logger.LogInformation("Atualização de usuário solicitada - ID: {UserId}", id);

            // Validar dados de entrada
            var validationResult = await _updateUserValidator.ValidateAsync(updateUserDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Atualização de usuário com dados inválidos - ID: {UserId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Atualizar usuário
            var updatedUser = await _userService.UpdateAsync(id, updateUserDto);
            
            _logger.LogInformation("Usuário atualizado com sucesso - ID: {UserId}", id);
            
            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuário atualizado com sucesso",
                Data = updatedUser
            });
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Tentativa de atualização de usuário inexistente - ID: {UserId}", id);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Usuário não encontrado",
                Errors = new List<string> { $"Usuário com ID {id} não foi encontrado" }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Atualização de usuário com dados duplicados - ID: {UserId} - {Message}", id, ex.Message);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = "Dados já cadastrados",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante atualização de usuário - ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Desativar usuário (soft delete)
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Status da desativação</returns>
    [HttpDelete("{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUserSoft(Guid id)
    {
        try
        {
            _logger.LogInformation("Desativação de usuário solicitada - ID: {UserId}", id);

            var result = await _userService.DeactivateAsync(id);
            if (!result)
            {
                _logger.LogWarning("Tentativa de desativação de usuário inexistente - ID: {UserId}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuario não encontrado",
                    Errors = new List<string> { $"Usuário com ID {id} não foi encontrado" }
                });
            }

            _logger.LogInformation("Usuário desativado com sucesso - ID: {UserId}", id);
            
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Usuário desativado com sucesso",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante remoção de usuário - ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Ativar usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Status da ativação</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(Guid id)
    {
        try
        {
            _logger.LogInformation("Ativação de usuário solicitada - ID: {UserId}", id);

            var result = await _userService.ActivateAsync(id);
            if (!result)
            {
                _logger.LogWarning("Tentativa de ativação de usuário inexistente - ID: {sendUserId}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não encontrado",
                    Errors = new List<string> { $"Usuário com ID {id} não foi encontrado" }
                });
            }

            _logger.LogInformation("Usuário ativado com sucesso - ID: {UserId}", id);
            
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Usuário ativado com sucesso",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante ativação de usuário - ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }


    /// <summary>
    /// Listar endereços de um usuário específico (rota de conveniência)
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de endereços do usuário</returns>
    [HttpGet("{userId}/addresses")]
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
            _logger.LogError(ex, "Erro durante busca de endereços do usuário - UserId: {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado. Tente novamente." }
            });
        }
    }

    /// <summary>
    /// Deletar usuário permanentemente
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Status da deleção</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var requesterUserId = HttpContext.User.FindFirst("sub")?.Value;

        try
        {
            _logger.LogInformation("BeginDeleteUser - Deleção permanente de usuário solicitada - UserId: {UserId}, RequestedBy: {RequesterUserId}", 
                id, requesterUserId);

            // Verificar se o usuário existe
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                stopwatch.Stop();
                _logger.LogWarning("DeleteUserNotFound - Usuário não encontrado para deleção - UserId: {UserId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuário não encontrado",
                    Errors = new List<string> { $"Usuário com ID {id} não foi encontrado" }
                });
            }

            // Não permitir deletar o próprio usuário
            if (requesterUserId != null && new Guid(requesterUserId) == id)
            {
                stopwatch.Stop();
                _logger.LogWarning("DeleteUserSelfNotAllowed - Tentativa de deletar próprio usuário - UserId: {UserId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Não é possível deletar seu próprio usuário",
                    Errors = new List<string> { "Você não pode deletar sua própria conta" }
                });
            }

            // Não permitir deletar usuário admin
            if (user.Email.Equals("admin@cadplus.com.br", StringComparison.OrdinalIgnoreCase))
            {
                stopwatch.Stop();
                _logger.LogWarning("DeleteAdminNotAllowed - Tentativa de deletar usuário administrador - UserId: {UserId}, Duration: {Duration}ms", 
                    id, stopwatch.ElapsedMilliseconds);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Não é possível deletar o usuário administrador",
                    Errors = new List<string> { "O usuário administrador não pode ser deletado" }
                });
            }

            _logger.LogDebug("DeleteUserValidation - Validações passaram, iniciando deleção - UserId: {UserId}", id);

            // Primeiro, deletar endereços relacionados
            var addresses = await _context.Addresses.Where(a => a.UserId == id).ToListAsync();
            if (addresses.Any())
            {
                _logger.LogInformation("DeleteUserAddresses - Removendo {Count} endereços do usuário - UserId: {UserId}", 
                    addresses.Count, id);
                _context.Addresses.RemoveRange(addresses);
            }

            // Depois, deletar o usuário
            var userEntity = await _context.Users.FindAsync(id);
            if (userEntity != null)
            {
                _context.Users.Remove(userEntity);
                await _context.SaveChangesAsync();
            }

            stopwatch.Stop();
            _logger.LogInformation("DeleteUserSuccess - Usuário deletado permanentemente com sucesso - UserId: {UserId}, Duration: {Duration}ms, RequestedBy: {RequesterUserId}", 
                id, stopwatch.ElapsedMilliseconds, requesterUserId);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Usuário deletado com sucesso",
                Data = true
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DeleteUserError - Erro durante deleção permanente de usuário - UserId: {UserId}, Duration: {Duration}ms", 
                id, stopwatch.ElapsedMilliseconds);
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor",
                Errors = new List<string> { "Ocorreu um erro inesperado durante a deleção. Tente novamente." }
            });
        }
    }
}

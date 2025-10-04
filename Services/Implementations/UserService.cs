using CadPlus.Data;
using CadPlus.DTOs;
using CadPlus.Models;
using Microsoft.EntityFrameworkCore;
using CadPlus.Extensions;

namespace CadPlus.Services.Implementations;

/// <summary>
/// Implementação do serviço de usuário
/// </summary>
public class UserService : IUserService
{
    private readonly CadPlusDbContext _context;
    private readonly ICpfValidationService _cpfValidationService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        CadPlusDbContext context,
        ICpfValidationService cpfValidationService,
        IPasswordService passwordService,
        ILogger<UserService> logger)
    {
        _context = context;
        _cpfValidationService = cpfValidationService;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Buscando usuário por ID: {UserId}", id);

        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado - ID: {UserId}", id);
            return null;
        }

        _logger.LogInformation("Usuário encontrado - ID: {UserId}, Email: {Email}", id, user.Email);

        return user.ToDto();
    }

    public async Task<PagedResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        _logger.LogInformation("Listando usuários - Página: {Page}, Tamanho: {PageSize}, Busca: {Search}", 
                             page, pageSize, search);

        var query = _context.Users
            .Include(u => u.Addresses)
            .AsQueryable();

        // Aplicar filtro de busca
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLowerInvariant();
            query = query.Where(u => 
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                u.Cpf.Contains(searchTerm));
        }

        // Contar total
        var totalCount = await query.CountAsync();

        // Aplicar paginação
        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = users.Select(u => u.ToDto()).ToList();

        _logger.LogInformation("Listagem concluída - Total: {TotalCount}, Retornados: {Count}", 
                             totalCount, userDtos.Count);

        return new PagedResponse<UserDto>
        {
            Data = userDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
    {
        _logger.LogInformation("Criando usuário: {Email}", createUserDto.Email);

        try
        {
            // Normalizar dados
            var email = createUserDto.Email.Trim().ToLowerInvariant();
            var cpf = _cpfValidationService.Clean(createUserDto.Cpf);

            // Validar constraints únicos
            await ValidateUniqueConstraintsForCreateAsync(email, cpf);

            // Criar usuário
            var user = new User
            {
                FirstName = createUserDto.FirstName.Trim(),
                LastName = createUserDto.LastName.Trim(),
                Cpf = cpf,
                Email = email,
                Phone = createUserDto.Phone.Trim(),
                PasswordHash = _passwordService.HashPassword(createUserDto.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Adicionar endereços
            if (createUserDto.Addresses?.Any() == true)
            {
                foreach (var addressDto in createUserDto.Addresses)
                {
                    var address = new Address
                    {
                        UserId = user.Id,
                        Street = addressDto.Street.Trim(),
                        Number = !string.IsNullOrWhiteSpace(addressDto.Number) ? addressDto.Number.Trim() : null,
                        Neighborhood = !string.IsNullOrWhiteSpace(addressDto.Neighborhood) ? addressDto.Neighborhood.Trim() : null,
                        Complement = addressDto.Complement?.Trim(),
                        City = addressDto.City.Trim(),
                        State = addressDto.State.Trim().ToUpperInvariant(),
                        ZipCode = addressDto.ZipCode.Trim(),
                        Country = !string.IsNullOrWhiteSpace(addressDto.Country) ? addressDto.Country.Trim() : "Brasil",
                        IsPrimary = addressDto.IsPrimary,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Addresses.Add(address);
                }
                
                await _context.SaveChangesAsync();
            }

            // Reload para incluir endereços
            var createdUser = await _context.Users
                .Include(u => u.Addresses)
                .FirstAsync(u => u.Id == user.Id);

            _logger.LogInformation("Usuário criado com sucesso - ID: {UserId}, Email: {Email}", 
                                 user.Id, email);

            return createdUser.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante criação de usuário: {Email}", createUserDto.Email);
            throw;
        }
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto)
    {
        _logger.LogInformation("Atualizando usuário - ID: {UserId}", id);

        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para atualização - ID: {UserId}", id);
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");
        }

        // Validar constraints únicos se necessário
        if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && updateUserDto.Email != user.Email)
        {
            await ValidateEmailUniqueAsync(updateUserDto.Email.Trim().ToLowerInvariant(), id);
        }

        // Atualizar campos
        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName) && updateUserDto.FirstName != user.FirstName)
        {
            user.FirstName = updateUserDto.FirstName.Trim();
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.LastName) && updateUserDto.LastName != user.LastName)
        {
            user.LastName = updateUserDto.LastName.Trim();
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && updateUserDto.Email != user.Email)
        {
            user.Email = updateUserDto.Email.Trim().ToLowerInvariant();
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Phone) && updateUserDto.Phone != user.Phone)
        {
            user.Phone = updateUserDto.Phone.Trim();
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
        {
            user.PasswordHash = _passwordService.HashPassword(updateUserDto.Password);
            hasChanges = true;
        }

        if (hasChanges)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Usuário atualizado com sucesso - ID: {UserId}", id);

        return user.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Removendo usuário - ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para remoção - ID: {UserId}", id);
            return false;
        }

        // Soft delete - apenas desativar
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuário removido (desativado) com sucesso - ID: {UserId}", id);

        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        _logger.LogInformation("Ativando usuário - ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para ativação - ID: {UserId}", id);
            return false;
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuário ativado com sucesso - ID: {UserId}", id);

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        _logger.LogInformation("Desativando usuário - ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para desativação - ID: {UserId}", id);
            return false;
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuário desativado com sucesso - ID: {UserId}", id);

        return true;
    }

    public async Task<bool> CpfExistsAsync(string cpf, Guid? excludeUserId = null)
    {
        var cleanCpf = _cpfValidationService.Clean(cpf);
        
        var query = _context.Users.AsQueryable();
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(u => u.Cpf == cleanCpf);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        
        var query = _context.Users.AsQueryable();
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<UserDto?> GetByCpfAsync(string cpf)
    {
        var cleanCpf = _cpfValidationService.Clean(cpf);

        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Cpf == cleanCpf);

        return user?.ToDto();
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        return user?.ToDto();
    }

    /// <summary>
    /// Valida constraints únicos para criação de usuário
    /// </summary>
    private async Task ValidateUniqueConstraintsForCreateAsync(string email, string cpf)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        if (emailExists)
        {
            throw new InvalidOperationException("E-mail já cadastrado");
        }

        var cpfExists = await _context.Users.AnyAsync(u => u.Cpf == cpf);
        if (cpfExists)
        {
            throw new InvalidOperationException("CPF já cadastrado");
        }
    }

    /// <summary>
    /// Valida se email é único (excluindo o usuário atual)
    /// </summary>
    private async Task ValidateEmailUniqueAsync(string email, Guid excludeUserId)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Id != excludeUserId && u.Email.ToLower() == email.ToLower());
        if (emailExists)
        {
            throw new InvalidOperationException("E-mail já cadastrado");
        }
    }
}

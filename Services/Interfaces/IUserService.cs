using CadPlus.DTOs;

namespace CadPlus.Services;

/// <summary>
/// Serviço responsável pelas operações de usuário
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Busca usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Dados do usuário ou null se não encontrado</returns>
    Task<UserDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Lista usuários com paginação
    /// </summary>
    /// <param name="page">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="search">Termo de busca opcional</param>
    /// <returns>Lista paginada de usuários</returns>
    Task<PagedResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null);
    
    /// <summary>
    /// Cria novo usuário
    /// </summary>
    /// <param name="createUserDto">Dados do usuário</param>
    /// <returns>Usuário criado</returns>
    Task<UserDto> CreateAsync(CreateUserDto createUserDto);
    
    /// <summary>
    /// Atualiza usuário existente
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="updateUserDto">Dados a serem atualizados</param>
    /// <returns>Usuário atualizado</returns>
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto);
    
    /// <summary>
    /// Remove usuário (soft delete)
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>True se removido com sucesso</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Ativa usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>True se ativado com sucesso</returns>
    Task<bool> ActivateAsync(Guid id);
    
    /// <summary>
    /// Desativa usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>True se desativado com sucesso</returns>
    Task<bool> DeactivateAsync(Guid id);
    
    /// <summary>
    /// Verifica se CPF já existe
    /// </summary>
    /// <param name="cpf">CPF a ser verificado</param>
    /// <param name="excludeUserId">ID do usuário a ser excluído da verificação</param>
    /// <returns>True se CPF já existe</returns>
    Task<bool> CpfExistsAsync(string cpf, Guid? excludeUserId = null);
    
    /// <summary>
    /// Verifica se email já existe
    /// </summary>
    /// <param name="email">Email a ser verificado</param>
    /// <param name="excludeUserId">ID do usuário a ser excluído da verificação</param>
    /// <returns>True se email já existe</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    
    /// <summary>
    /// Busca usuário por CPF
    /// </summary>
    /// <param name="cpf">CPF do usuário</param>
    /// <returns>Dados do usuário ou null se não encontrado</returns>
    Task<UserDto?> GetByCpfAsync(string cpf);
    
    /// <summary>
    /// Busca usuário por email
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <returns>Dados do usuário ou null se não encontrado</returns>
    Task<UserDto?> GetByEmailAsync(string email);
}

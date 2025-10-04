using AutoMapper;
using CadPlus.DTOs;
using CadPlus.Models;
using CadPlus.Extensions;

namespace CadPlus.Mappings;

/// <summary>
/// Perfil Automapper para entidade User
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        // User -> UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // UserDto -> User (para updates)
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Addresses, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());

        // CreateUserDto -> User
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => HashPassword(src.Password)))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => CleanCpf(src.Cpf)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLowerInvariant().Trim()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Trim()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Trim()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Addresses, opt => opt.Ignore()) // Será tratado separadamente
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());

        // RegisterDto -> User (reusa CreateUserDto mapping)
        CreateMap<RegisterDto, User>()
            .IncludeBase<CreateUserDto, User>(); // Herda mapeamento de CreateUserDto

        // UpdateUserDto -> User (apenas campos específicos)
        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Password) ? HashPassword(src.Password) : null))
            .ForMember(dest => dest.Cpf, opt => opt.Ignore()) // CPF não pode ser alterado
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Email) ? src.Email.ToLowerInvariant().Trim() : null))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.FirstName) ? src.FirstName.Trim() : null))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.LastName) ? src.LastName.Trim() : null))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Phone) ? src.Phone.Trim() : null))
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Addresses, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());

        // Address -> AddressDto
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street))
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
            .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
            .ForMember(dest => dest.Complement, opt => opt.MapFrom(src => src.Complement))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary));

        // AddressDto -> Address
        CreateMap<AddressDto, Address>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // CreateAddressDto -> Address
        CreateMap<CreateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street.Trim()))
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Number) ? src.Number.Trim() : null))
            .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Neighborhood) ? src.Neighborhood.Trim() : null))
            .ForMember(dest => dest.Complement, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Complement) ? src.Complement.Trim() : null))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City.Trim()))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.Trim().ToUpperInvariant()))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode.Trim()))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Country) ? src.Country.Trim() : "Brasil"))
            .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // UpdateAddressDto -> Address
        CreateMap<UpdateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Street) ? src.Street.Trim() : null))
            .ForMember(dest => dest.Complement, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Complement) ? src.Complement.Trim() : null))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.City) ? src.City.Trim() : null))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.State) ? src.State.Trim().ToUpperInvariant() : null))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.ZipCode) ? src.ZipCode.Trim() : null))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => 
                !string.IsNullOrWhiteSpace(src.Country) ? src.Country.Trim() : null))
            .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // AuditLog -> AuditLogDto (se necessário no futuro)
        CreateMap<AuditLog, object>() // Mapeamento para logs de auditoria
            .ForMember(dest => dest.ToString(), opt => opt.Ignore());
    }

    /// <summary>
    /// Resolver para limpar CPF
    /// </summary>
    /// <param name="cpf">CPF com formatação</param>
    /// <returns>CPF limpo</returns>
    private static string CleanCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return string.Empty;

        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Resolver para hash de senha usando BCrypt
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash da senha</returns>
    private static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password não pode ser vazio", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}

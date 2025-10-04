using CadPlus.Models;
using CadPlus.DTOs;

namespace CadPlus.Extensions;

/// <summary>
/// Extensões para entidade User
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Converte User para UserDto
    /// </summary>
    public static UserDto ToDto(this User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Cpf = user.Cpf,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Addresses = user.Addresses.Select(a => new AddressDto
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
            }).ToList()
        };
    }

    /// <summary>
    /// Converte CreateUserDto para User
    /// </summary>
    public static User ToEntity(this CreateUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.ToLowerInvariant().Trim(),
            Cpf = dto.Cpf.Trim(),
            Phone = dto.Phone.Trim(),
            PasswordHash = string.Empty, // Será definido pelo service
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Addresses = dto.Addresses.Select(a => new Address
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty, // Será definido após criar o user
                Street = a.Street,
                Complement = a.Complement,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                Country = a.Country ?? "Brasil",
                IsPrimary = a.IsPrimary,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        // Correção do UserId nas Addresses após criação do User
        foreach (var address in user.Addresses)
        {
            address.UserId = user.Id;
        }

        return user;
    }

    /// <summary>
    /// Converte UserRegisterDto para User
    /// </summary>
    public static User ToEntity(this RegisterDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.ToLowerInvariant().Trim(),
            Cpf = dto.Cpf.Trim(),
            Phone = dto.Phone.Trim(),
            PasswordHash = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Addresses = dto.Addresses.Select(a => new Address
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty,
                Street = a.Street,
                Complement = a.Complement,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                Country = a.Country ?? "Brasil",
                IsPrimary = a.IsPrimary,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };
    }

    /// <summary>
    /// Aplica update em User existente
    /// </summary>
    public static void ApplyUpdate(this User user, UpdateUserDto updateDto)
    {
        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto));

        if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            user.FirstName = updateDto.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            user.LastName = updateDto.LastName.Trim();

        if (!string.IsNullOrWhiteSpace(updateDto.Email))
            user.Email = updateDto.Email.ToLowerInvariant().Trim();

        if (!string.IsNullOrWhiteSpace(updateDto.Phone))
            user.Phone = updateDto.Phone.Trim();

        user.UpdatedAt = DateTime.UtcNow;
    }
}

using CadPlus.DTOs;
using CadPlus.Models;
using Bogus;

namespace CadPlus.Tests.TestUtilities;

/// <summary>
/// Builder para dados de teste usando Bogus
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker _faker = new("pt_BR");

    /// <summary>
    /// Cria um DTO de registro válido
    /// </summary>
    public static RegisterDto CreateValidRegisterDto()
    {
        return new RegisterDto
        {
            FirstName = _faker.Person.FirstName,
            LastName = _faker.Person.LastName,
            Email = _faker.Person.Email.ToLowerInvariant(),
            Password = "MySecureP@ssw0rd!",
            ConfirmPassword = "MySecureP@ssw0rd!",
            Phone = _faker.Phone.PhoneNumber(),
            Cpf = "11144477735", // CPF válido fixo para testes
            Addresses = new List<CreateAddressDto>
            {
                new CreateAddressDto
                {
                    Street = _faker.Address.StreetAddress(),
                    Complement = _faker.Address.SecondaryAddress(),
                    City = _faker.Address.City(),
                    State = _faker.Address.StateAbbr(),
                    ZipCode = _faker.Address.ZipCode(),
                    Country = "Brasil",
                    IsPrimary = true
                }
            }
        };
    }

    /// <summary>
    /// Cria um DTO de login válido
    /// </summary>
    public static LoginDto CreateValidLoginDto()
    {
        return new LoginDto
        {
            Email = _faker.Person.Email.ToLowerInvariant(),
            Password = "TestPassword123!"
        };
    }

    /// <summary>
    /// Cria um usuário válido para testes
    /// </summary>
    public static User CreateValidUser(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? cpf = null,
        string? phone = null)
    {
        var first = firstName ?? _faker.Person.FirstName;
        var last = lastName ?? _faker.Person.LastName;
        var mail = email ?? _faker.Person.Email.ToLowerInvariant();
        var document = cpf ?? _faker.Random.GenerateValidCpf();
        var tel = phone ?? _faker.Phone.PhoneNumber();

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = first,
            LastName = last,
            Email = mail,
            Cpf = document,
            Phone = tel,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Addresses = new List<Address>
            {
                new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Street = _faker.Address.StreetAddress(),
                    City = _faker.Address.City(),
                    State = _faker.Address.StateAbbr(),
                    ZipCode = _faker.Address.ZipCode(),
                    Country = "Brasil",
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
    }

    /// <summary>
    /// Cria um CreateUserDto válido
    /// </summary>
    public static CreateUserDto CreateValidCreateUserDto()
    {
        return new CreateUserDto
        {
            FirstName = _faker.Person.FirstName,
            LastName = _faker.Person.LastName,
            Cpf = _faker.Random.GenerateValidCpf(),
            Email = _faker.Person.Email.ToLowerInvariant(),
            Phone = _faker.Phone.PhoneNumber(),
            Password = "MySecureP@ssw0rd!",
            ConfirmPassword = "MySecureP@ssw0rd!",
            Addresses = new List<CreateAddressDto>
            {
                new CreateAddressDto
                {
                    Street = _faker.Address.StreetAddress(),
                    City = _faker.Address.City(),
                    State = _faker.Address.StateAbbr(),
                    ZipCode = _faker.Address.ZipCode(),
                    Country = "Brasil",
                    IsPrimary = true
                }
            }
        };
    }

    /// <summary>
    /// Cria uma lista de usuários para testes
    /// </summary>
    public static List<User> CreateValidUsers(int count = 5)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateValidUser())
            .ToList();
    }

    /// <summary>
    /// Cria um CPF válido
    /// </summary>
    public static string CreateValidCpf()
    {
        return _faker.Random.GenerateValidCpf();
    }

    /// <summary>
    /// Cria uma senha válida
    /// </summary>
    public static string CreateValidPassword()
    {
        return _faker.Internet.Password(12, prefix: "Test!");
    }
}

/// <summary>
/// Extensões para FakeDataGenerator para CPF brasileiro
/// </summary>
public static class FakerExtensions
{
    public static string GenerateValidCpf(this Randomizer randomizer)
    {
        var numbers = randomizer.Digits(9, 0, 9);
        
        // Calcula os dígitos verificadores
        var sum1 = 0;
        for (int i = 0; i < 9; i++)
        {
            sum1 += numbers[i] * (10 - i);
        }
        var digit1 = sum1 % 11 < 2 ? 0 : 11 - (sum1 % 11);
        
        var sum2 = 0;
        for (int i = 0; i < 9; i++)
        {
            sum2 += numbers[i] * (11 - i);
        }
        sum2 += digit1 * 2;
        var digit2 = sum2 % 11 < 2 ? 0 : 11 - (sum2 % 11);
        
        return string.Join("", numbers) + digit1 + digit2;
    }
}

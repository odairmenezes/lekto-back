using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using CadPlus.Data;
using CadPlus.Services.Implementations;
using CadPlus.Services;
using CadPlus.Models;
using CadPlus.DTOs;
using CadPlus.Tests.TestUtilities;
using Xunit;

namespace CadPlus.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly CadPlusDbContext _context;
    private readonly UserService _service;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ICpfValidationService> _cpfValidationServiceMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<CadPlusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CadPlusDbContext(options);
        
        _passwordServiceMock = new Mock<IPasswordService>();
        _cpfValidationServiceMock = new Mock<ICpfValidationService>();
        _loggerMock = new Mock<ILogger<UserService>>();

        // Setup CPF service to return cleaned CPF
        _cpfValidationServiceMock.Setup(x => x.Clean(It.IsAny<string>()))
            .Returns((string cpf) => cpf.Replace(".", "").Replace("-", "").Replace(" ", ""));

        _service = new UserService(_context, _cpfValidationServiceMock.Object, _passwordServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnUserDto()
    {
        // Arrange
        var createUserDto = TestDataBuilder.CreateValidCreateUserDto();

        _passwordServiceMock.Setup(x => x.HashPassword(createUserDto.Password))
            .Returns("hashed_password");

        // Act
        var result = await _service.CreateAsync(createUserDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be(createUserDto.FirstName);
        result.LastName.Should().Be(createUserDto.LastName);
        result.Email.Should().Be(createUserDto.Email);
        result.Cpf.Should().Be(createUserDto.Cpf);
        result.Phone.Should().Be(createUserDto.Phone);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        _passwordServiceMock.Verify(x => x.HashPassword(createUserDto.Password), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldSaveToDatabase()
    {
        // Arrange
        var createUserDto = TestDataBuilder.CreateValidCreateUserDto();

        _passwordServiceMock.Setup(x => x.HashPassword(createUserDto.Password))
            .Returns("hashed_password");

        // Act
        var result = await _service.CreateAsync(createUserDto);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == result.Id);
        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be(createUserDto.FirstName);
        savedUser.LastName.Should().Be(createUserDto.LastName);
        savedUser.Email.Should().Be(createUserDto.Email);
        savedUser.Cpf.Should().Be(createUserDto.Cpf);
        savedUser.Phone.Should().Be(createUserDto.Phone);
        savedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithNumberAndNeighborhood_ShouldSaveBothFields()
    {
        // Arrange
        var createUserDto = TestDataBuilder.CreateValidCreateUserDto();
        var addressDto = createUserDto.Addresses.First();
        addressDto.Number = "123";
        addressDto.Neighborhood = "Centro";

        _passwordServiceMock.Setup(x => x.HashPassword(createUserDto.Password))
            .Returns("hashed_password");

        // Act
        var result = await _service.CreateAsync(createUserDto);

        // Assert
        result.Should().NotBeNull();
        result.Addresses.Should().HaveCount(1);
        
        var address = result.Addresses.First();
        address.Number.Should().Be("123");
        address.Neighborhood.Should().Be("Centro");
        address.Street.Should().Be(addressDto.Street);
        address.City.Should().Be(addressDto.City);
        address.State.Should().Be(addressDto.State);
        address.ZipCode.Should().Be(addressDto.ZipCode);
    }

    [Fact]
    public async Task CreateAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateValidUser();
        var createUserDto = TestDataBuilder.CreateValidCreateUserDto();
        createUserDto.Email = existingUser.Email;
        
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(x => x.HashPassword(createUserDto.Password))
            .Returns("hashed_password");

        // Act & Assert
        var action = async () => await _service.CreateAsync(createUserDto);
        await action.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("E-mail já cadastrado");

        _passwordServiceMock.Verify(x => x.HashPassword(createUserDto.Password), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithExistingCpf_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateValidUser();
        var createUserDto = TestDataBuilder.CreateValidCreateUserDto();
        createUserDto.Cpf = existingUser.Cpf;
        
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        _passwordServiceMock.Setup(x => x.HashPassword(createUserDto.Password))
            .Returns("hashed_password");

        // Act & Assert
        var action = async () => await _service.CreateAsync(createUserDto);
        await action.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("CPF já cadastrado");

        _passwordServiceMock.Verify(x => x.HashPassword(createUserDto.Password), Times.Never);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUserDto()
    {
        // Arrange
        var user = TestDataBuilder.CreateValidUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByEmailAsync(user.Email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Cpf.Should().Be(user.Cpf);
        result.Phone.Should().Be(user.Phone);
        result.IsActive.Should().Be(user.IsActive);
        result.CreatedAt.Should().Be(user.CreatedAt);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var result = await _service.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithEmptyEmail_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByEmailAsync("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CpfExistsAsync_WithExistingCpf_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateValidUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CpfExistsAsync(user.Cpf);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CpfExistsAsync_WithNonExistentCpf_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentCpf = "11122233344";

        // Act
        var result = await _service.CpfExistsAsync(nonExistentCpf);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CpfExistsAsync_WithExcludeUserId_ShouldExcludeUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var cpf = TestDataBuilder.CreateValidCpf();

        var user1 = TestDataBuilder.CreateValidUser(email: "user1@test.com", cpf: cpf);
        user1.Id = userId1;

        var user2 = TestDataBuilder.CreateValidUser(email: "user2@test.com", cpf: "22233344455");
        user2.Id = userId2;

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CpfExistsAsync(cpf, userId1);

        // Assert
        result.Should().BeFalse(); // Should not find user1 because it's excluded
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateValidUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.EmailExistsAsync(user.Email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithDifferentCases_ShouldMatchRegardless()
    {
        // Arrange
        var user = TestDataBuilder.CreateValidUser(email: "test@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        var testEmails = new[] { "test@example.com", "TEST@EXAMPLE.COM", "Test@Example.Com" };
        foreach (var testEmail in testEmails)
        {
            var result = await _service.EmailExistsAsync(testEmail);
            result.Should().BeTrue(); // Should match regardless of case
        }
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var result = await _service.EmailExistsAsync(nonExistentEmail);

        // Assert
        result.Should().BeFalse();
    }
}
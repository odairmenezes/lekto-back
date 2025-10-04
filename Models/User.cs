using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CadPlus.Models;

/// <summary>
/// Entidade que representa um usuário do sistema Cad+ ERP
/// </summary>
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    [Required]
    [StringLength(11)]
    [Column(TypeName = "CHAR(11)")]
    public string Cpf { get; set; } = string.Empty;
    
    [Required]
    [StringLength(254)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(15)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

/// <summary>
/// Entidade que representa um endereço de um usuário
/// </summary>
public class Address
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;
    
    [StringLength(15)]
    public string? Number { get; set; }
    
    [StringLength(50)]
    public string? Neighborhood { get; set; }
    
    [StringLength(100)]
    public string? Complement { get; set; }
    
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2)]
    [Column(TypeName = "CHAR(2)")]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string ZipCode { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Country { get; set; } = "Brasil";
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

/// <summary>
/// Entidade que registra logs de alterações no sistema
/// </summary>
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    public Guid EntityId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    public string? OldValue { get; set; }
    
    public string? NewValue { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    public Guid ChangedBy { get; set; }
    
    [StringLength(45)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

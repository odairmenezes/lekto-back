using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CadPlus.Models;

namespace CadPlus.Data;

/// <summary>
/// Contexto do Entity Framework para o sistema Cad+ ERP
/// </summary>
public class CadPlusDbContext : DbContext
{
    public CadPlusDbContext(DbContextOptions<CadPlusDbContext> options)
        : base(options) 
    {
        // Configurar para melhor performance e reduzir problemas de concorrência
        ChangeTracker.AutoDetectChangesEnabled = true;
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    /// <summary>
    /// Tabela de usuários
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Tabela de endereços
    /// </summary>
    public DbSet<Address> Addresses { get; set; }

    /// <summary>
    /// Tabela de logs de auditoria
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureAddress(modelBuilder);
        ConfigureAuditLog(modelBuilder);
    }

    /// <summary>
    /// Configurações específicas da entidade User
    /// </summary>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Chave primária
            entity.HasKey(u => u.Id);

            // Índices únicos
            entity.HasIndex(u => u.Cpf)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Cpf_Unique");

            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email_Unique");

            // Configurações de propriedades
            entity.Property(u => u.FirstName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.LastName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.Cpf)
                  .IsRequired()
                  .HasMaxLength(11)
                  .IsFixedLength();

            entity.Property(u => u.Email)
                  .IsRequired()
                  .HasMaxLength(254);

            entity.Property(u => u.Phone)
                  .IsRequired()
                  .HasMaxLength(15);

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.Property(u => u.IsActive)
                  .HasDefaultValue(true);

            entity.Property(u => u.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Timestamps automáticos
            entity.Property(u => u.UpdatedAt)
                  .HasDefaultValue(null);

            // Relacionamentos
            entity.HasMany(u => u.Addresses)
                  .WithOne(a => a.User)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.AuditLogs)
                  .WithOne(al => al.User)
                  .HasForeignKey(al => al.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configurações específicas da entidade Address
    /// </summary>
    private static void ConfigureAddress(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            // Chave primária
            entity.HasKey(a => a.Id);

            // Chave estrangeira
            entity.Property(a => a.UserId)
                  .IsRequired();

            // Configurações de propriedades
            entity.Property(a => a.Street)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(a => a.Number)
                  .HasMaxLength(15);

            entity.Property(a => a.Neighborhood)
                  .HasMaxLength(50);

            entity.Property(a => a.Complement)
                  .HasMaxLength(100);

            entity.Property(a => a.City)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(a => a.State)
                  .IsRequired()
                  .HasMaxLength(2)
                  .IsFixedLength();

            entity.Property(a => a.ZipCode)
                  .IsRequired()
                  .HasMaxLength(10);

            entity.Property(a => a.Country)
                  .HasMaxLength(100)
                  .HasDefaultValue("Brasil");

            entity.Property(a => a.IsPrimary)
                  .HasDefaultValue(false);

            entity.Property(a => a.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relacionamento com User
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Addresses)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Restrição: apenas um endereço principal por usuário
            entity.HasIndex(a => new { a.UserId, a.IsPrimary })
                  .HasFilter("[IsPrimary] = 1")
                  .HasDatabaseName("IX_Addresses_UniquePrimaryPerUser");
        });
    }

    /// <summary>
    /// Configurações específicas da entidade AuditLog
    /// </summary>
    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            // Chave primária
            entity.HasKey(al => al.Id);

            // Configurações de propriedades
            entity.Property(al => al.UserId)
                  .IsRequired();

            entity.Property(al => al.EntityType)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(al => al.EntityId)
                  .IsRequired();

            entity.Property(al => al.FieldName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(al => al.OldValue)
                  .HasColumnType("TEXT");

            entity.Property(al => al.NewValue)
                  .HasColumnType("TEXT");

            entity.Property(al => al.ChangedAt)
                  .IsRequired()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(al => al.ChangedBy)
                  .IsRequired();

            entity.Property(al => al.IpAddress)
                  .HasMaxLength(45);

            entity.Property(al => al.UserAgent)
                  .HasMaxLength(500);

            // Relacionamento com User
            entity.HasOne(al => al.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(al => al.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para performance
            entity.HasIndex(al => al.ChangedAt)
                  .HasDatabaseName("IX_AuditLogs_ChangedAt");

            entity.HasIndex(al => new { al.EntityType, al.EntityId })
                  .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

            entity.HasIndex(al => al.ChangedBy)
                  .HasDatabaseName("IX_AuditLogs_ChangedBy");
        });
    }

    /// <summary>
    /// Override para interceptar operações e gerar logs de auditoria
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auditoria temporariamente desabilitada para evitar problemas durante inicialização
        // TODO: Reativar auditoria quando o sistema de usuários estiver funcionando
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Intercepta mudanças e cria logs de auditoria
    /// </summary>
    private Task AuditChanges()
    {
        var auditEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Select(e => new AuditEntry(e))
            .Where(e => e.HasChanges)
            .ToList();

        foreach (var entry in auditEntries)
        {
            foreach (var change in entry.Changes)
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = entry.UserId, // Será injetado pelo service
                    EntityType = entry.EntityType,
                    EntityId = entry.EntityId,
                    FieldName = change.FieldName,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = entry.ChangedBy, // Será injetado pelo service
                    IpAddress = entry.IpAddress,
                    UserAgent = entry.UserAgent
                };

                AuditLogs.Add(auditLog);
            }
        }
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// Classe auxiliar para capturar mudanças nas entidades
/// </summary>
public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
        EntityType = entry.Entity.GetType().Name;
        Changes = new List<AuditChange>();
        HasChanges = false;
    }

    public EntityEntry Entry { get; }
    public string EntityType { get; }
    public List<AuditChange> Changes { get; }
    public bool HasChanges { get; set; }
    public Guid UserId { get; set; }
    public Guid EntityId => (Guid)(Entry.IsKeySet ? Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue ?? Guid.Empty : Guid.Empty);
    public Guid ChangedBy { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public void AddChange(string propertyName, object? originalValue, object? currentValue)
    {
        Changes.Add(new AuditChange
        {
            FieldName = propertyName,
            OldValue = originalValue?.ToString(),
            NewValue = currentValue?.ToString()
        });
        HasChanges = true;
    }
}

/// <summary>
/// Representa uma mudança específica em um campo
/// </summary>
public class AuditChange
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

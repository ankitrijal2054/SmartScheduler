using Microsoft.EntityFrameworkCore;
using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Infrastructure.Persistence;

/// <summary>
/// Application database context for Entity Framework Core.
/// Configures all entities, relationships, constraints, and indexes.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext class.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Contractor> Contractors { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<DispatcherContractorList> DispatcherContractorLists { get; set; } = null!;

    /// <summary>
    /// Configures the model using the Fluent API.
    /// Defines relationships, constraints, and indexes.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(e => e.Role)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Unique index on Email
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // Index on Role for role-based queries
            entity.HasIndex(e => e.Role)
                .HasDatabaseName("IX_Users_Role");

            // Soft delete index (IsDeleted)
            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Users_IsDeleted");
        });

        // Contractor entity configuration
        modelBuilder.Entity<Contractor>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(e => e.Latitude)
                .IsRequired()
                .HasColumnType("numeric(10, 8)");

            entity.Property(e => e.Longitude)
                .IsRequired()
                .HasColumnType("numeric(11, 8)");

            entity.Property(e => e.TradeType)
                .IsRequired();

            entity.Property(e => e.WorkingHoursStart)
                .IsRequired()
                .HasColumnType("time");

            entity.Property(e => e.WorkingHoursEnd)
                .IsRequired()
                .HasColumnType("time");

            entity.Property(e => e.AverageRating)
                .HasColumnType("numeric(3, 2)");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Composite index for recommendation queries
            entity.HasIndex(e => new { e.IsActive, e.TradeType })
                .HasDatabaseName("IX_Contractors_IsActive_TradeType");
        });

        // Customer entity configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });

        // Job entity configuration
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CustomerId)
                .IsRequired();

            entity.Property(e => e.JobType)
                .IsRequired();

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(e => e.Latitude)
                .IsRequired()
                .HasColumnType("numeric(10, 8)");

            entity.Property(e => e.Longitude)
                .IsRequired()
                .HasColumnType("numeric(11, 8)");

            entity.Property(e => e.DesiredDateTime)
                .IsRequired();

            entity.Property(e => e.EstimatedDurationHours)
                .IsRequired()
                .HasColumnType("numeric(5, 2)");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2048);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(0); // JobStatus.Pending

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Composite index for job list queries
            entity.HasIndex(e => new { e.Status, e.DesiredDateTime })
                .HasDatabaseName("IX_Jobs_Status_DesiredDateTime");
        });

        // Assignment entity configuration
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.JobId)
                .IsRequired();

            entity.Property(e => e.ContractorId)
                .IsRequired();

            entity.Property(e => e.AssignedAt)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(0); // AssignmentStatus.Pending

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Composite index for availability checking
            entity.HasIndex(e => new { e.ContractorId, e.Status })
                .HasDatabaseName("IX_Assignments_ContractorId_Status");
        });

        // Review entity configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.JobId)
                .IsRequired();

            entity.Property(e => e.ContractorId)
                .IsRequired();

            entity.Property(e => e.CustomerId)
                .IsRequired();

            entity.Property(e => e.Rating)
                .IsRequired();

            entity.Property(e => e.Comment)
                .HasMaxLength(2048);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Unique index: one review per job
            entity.HasIndex(e => e.JobId)
                .IsUnique()
                .HasDatabaseName("IX_Reviews_JobId_Unique");
        });

        // DispatcherContractorList entity configuration
        modelBuilder.Entity<DispatcherContractorList>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DispatcherId)
                .IsRequired();

            entity.Property(e => e.ContractorId)
                .IsRequired();

            entity.Property(e => e.AddedAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });

        // Relationships configuration

        // User 1:1 ↔ Contractor (optional)
        modelBuilder.Entity<Contractor>()
            .HasOne(c => c.User)
            .WithOne(u => u.Contractor)
            .HasForeignKey<Contractor>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Contractors_Users");

        // User 1:1 ↔ Customer (optional)
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Customers_Users");

        // User 1:N ↔ DispatcherContractorList (optional)
        modelBuilder.Entity<DispatcherContractorList>()
            .HasOne(d => d.Dispatcher)
            .WithMany(u => u.DispatcherContractorLists)
            .HasForeignKey(d => d.DispatcherId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DispatcherContractorLists_Users");

        // Customer 1:N → Job
        modelBuilder.Entity<Job>()
            .HasOne(j => j.Customer)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Jobs_Customers");

        // Job 1:1 → Assignment (optional, one assignment per job)
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Job)
            .WithOne(j => j.Assignment)
            .HasForeignKey<Assignment>(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Assignments_Jobs");

        // Job 1:1 → Review (optional, one review per job)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Job)
            .WithOne(j => j.Review)
            .HasForeignKey<Review>(r => r.JobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Reviews_Jobs");

        // Contractor 1:N → Assignment
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Contractor)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.ContractorId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Assignments_Contractors");

        // Contractor 1:N → Review
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Contractor)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.ContractorId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Reviews_Contractors");

        // Customer 1:N → Review
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Reviews_Customers");

        // Contractor N:1 ← DispatcherContractorList
        modelBuilder.Entity<DispatcherContractorList>()
            .HasOne(d => d.Contractor)
            .WithMany(c => c.DispatcherContractorLists)
            .HasForeignKey(d => d.ContractorId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DispatcherContractorLists_Contractors");
    }
}


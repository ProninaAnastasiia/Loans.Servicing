using Loans.Servicing.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Servicing.Data;

public class OperationsDbContext: DbContext
{
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options) : base(options)
    {
    }
    
    public DbSet<OperationEntity> Operations { get; set; }
    public DbSet<StoredEvent> Events { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OperationEntity>().HasKey(u => u.OperationId);
        
        modelBuilder.Entity<StoredEvent>().HasKey(e => e.Id);

        modelBuilder.Entity<StoredEvent>().Property(e => e.Payload).HasColumnType("jsonb");
        
        base.OnModelCreating(modelBuilder);
    }

}
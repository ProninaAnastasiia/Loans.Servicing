using Loans.Servicing.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Servicing.Data;

public class OperationsDbContext: DbContext
{
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options) : base(options)
    {
    }
    
    public DbSet<OperationEntity> Operations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OperationEntity>().HasKey(u => u.OperationId);
        
        base.OnModelCreating(modelBuilder);
    }

}
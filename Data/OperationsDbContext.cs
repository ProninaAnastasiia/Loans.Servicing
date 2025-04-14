using System.Diagnostics.Contracts;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Process;
using Microsoft.EntityFrameworkCore;

namespace Loans.Servicing.Data;

public class OperationsDbContext: DbContext
{
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options) : base(options)
    {
    }
    
    /*public DbSet<ProcessEntity> Processes => Set<ProcessEntity>();
    public DbSet<ProcessStepEntity> Steps => Set<ProcessStepEntity>();*/
    
    public DbSet<Process.Process> Processes { get; set; }
    public DbSet<Step> Steps { get; set; }

}
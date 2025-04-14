using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Loans.Servicing.Data;

public class OperationsDbContext: DbContext
{
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options) : base(options)
    {
    }

}
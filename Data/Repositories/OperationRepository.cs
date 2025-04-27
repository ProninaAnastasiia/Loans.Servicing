using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Servicing.Data.Repositories;

public class OperationRepository : IOperationRepository
{
    private readonly OperationsDbContext _dbContext;
    private readonly ILogger<OperationRepository> _logger;

    public OperationRepository(OperationsDbContext dbContext, ILogger<OperationRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<OperationEntity?> GetByIdAsync(Guid operationId)
    {
        return await _dbContext.Operations.FirstOrDefaultAsync(p => p.OperationId == operationId);
    }

    public async Task SaveAsync(OperationEntity operation)
    {
        try
        {
            var existingProcess =
                await _dbContext.Operations.FirstOrDefaultAsync(p => p.OperationId == operation.OperationId);

            if (existingProcess == null)
            {
                _dbContext.Operations.Add(operation);
            }
            else
            {
                _dbContext.Entry(existingProcess).CurrentValues.SetValues(operation);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении операции создания договора");
            throw;
        }
    }

    public async Task UpdateStatusAsync(OperationEntity operation, OperationStatus status)
    {
        try
        {
            operation.Status = status;
            _dbContext.Operations.Update(operation);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса операции создания договора");
            throw;
        }
    }
}

using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;

namespace Loans.Servicing.Data.Repositories;

public interface IOperationRepository
{
    Task<OperationEntity?> GetByIdAsync(Guid operationId);
    Task SaveAsync(OperationEntity operation);
    Task UpdateStatusAsync(OperationEntity operation, OperationStatus status);
}
using Loans.Servicing.Process;

namespace Loans.Servicing.Data.Repositories;

public interface IProcessRepository
{
    Task<Process.Process> GetByIdAsync(Guid processId);
    Task SaveAsync(Process.Process process);
    Task AddStepAsync(Guid processId, Step step);
}
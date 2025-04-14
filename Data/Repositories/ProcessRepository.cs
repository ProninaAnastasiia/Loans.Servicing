using Loans.Servicing.Process;
using Microsoft.EntityFrameworkCore;

namespace Loans.Servicing.Data.Repositories;

public class ProcessRepository : IProcessRepository
{
    private readonly OperationsDbContext _dbContext;

    public ProcessRepository(OperationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Получение процесса по его идентификатору
    public async Task<Process.Process> GetByIdAsync(Guid processId)
    {
        return await _dbContext.Processes
            .Include(p => p.Steps)  // Загружаем шаги процесса
            .FirstOrDefaultAsync(p => p.Id == processId);
    }

    // Сохранение процесса (или его обновление)
    public async Task SaveAsync(Process.Process process)
    {
        var existingProcess = await _dbContext.Processes
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == process.Id);

        if (existingProcess == null)
        {
            _dbContext.Processes.Add(process);
        }
        else
        {
            _dbContext.Entry(existingProcess).CurrentValues.SetValues(process);
            // Обновляем шаги, если они были изменены
            foreach (var step in process.Steps)
            {
                var existingStep = existingProcess.Steps.FirstOrDefault(s => s.StepIndex == step.StepIndex);
                if (existingStep == null)
                {
                    existingProcess.Steps.Add(step);
                }
                else
                {
                    _dbContext.Entry(existingStep).CurrentValues.SetValues(step);
                }
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    // Добавление шага в процесс
    public async Task AddStepAsync(Guid processId, Step step)
    {
        var process = await _dbContext.Processes
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process == null)
        {
            throw new InvalidOperationException($"Процесс с ID {processId} не найден.");
        }

        process.Steps.Add(step);
        await _dbContext.SaveChangesAsync();
    }
}

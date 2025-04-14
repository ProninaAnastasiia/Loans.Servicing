using Loans.Servicing.Data.Repositories;

namespace Loans.Servicing.Process;

public class ProcessOrchestrator
{
    private readonly IProcessRepository _processRepository;
    private readonly IStepExecutor _stepExecutor;
    
    public ProcessOrchestrator(IProcessRepository processRepository, IStepExecutor stepExecutor)
    {
        _processRepository = processRepository;
        _stepExecutor = stepExecutor;
    }

    public async Task StartProcessAsync(Guid processId)
    {
        var process = await _processRepository.GetByIdAsync(processId);

        if (process == null || process.Status != "InProgress")
        {
            // Начинаем новый процесс
            process = new Process { Id = processId, Status = "InProgress" };
            await _processRepository.SaveAsync(process);
        }

        foreach (var step in process.Steps.Where(s => s.Status == "Pending"))
        {
            try
            {
                await ExecuteStepAsync(process, step);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                process.Status = "Failed";
                await _processRepository.SaveAsync(process);
                throw new InvalidOperationException("Ошибка в процессе", ex);
            }
        }

        process.Status = "Completed";
        await _processRepository.SaveAsync(process);
    }

    private async Task ExecuteStepAsync(Process process, Step step)
    {
        step.Status = "InProgress";
        await _processRepository.SaveAsync(process); // Обновляем статус шага

        try
        {
            switch (step.StepIndex)
            {
                case 1:
                    await _stepExecutor.ExecuteCreateContractAsync();
                    break;
                case 2:
                    await _stepExecutor.ExecuteCalculatePaymentScheduleAsync();
                    break;
                case 3:
                    await _stepExecutor.ExecuteCalculateIndebtednessAsync();
                    break;
                case 4:
                    await _stepExecutor.ExecuteSendContractToClientAsync();
                    break;
                case 5:
                    await _stepExecutor.ExecuteActivateLoanAsync();
                    break;
                default:
                    throw new InvalidOperationException("Неизвестный шаг");
            }

            step.Status = "Completed";
        }
        catch (Exception ex)
        {
            // Ошибка на шаге, возможно откатить
            step.Status = "Failed";
            await _processRepository.SaveAsync(process);
            throw new InvalidOperationException($"Ошибка на шаге {step.StepIndex}", ex);
        }

        await _processRepository.SaveAsync(process);
    }
}

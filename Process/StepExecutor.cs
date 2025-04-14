using Loans.Servicing.Data.Repositories;

namespace Loans.Servicing.Process;

public class StepExecutor
{
    private readonly IProcessRepository _processRepository;

    public StepExecutor(IProcessRepository processRepository)
    {
        _processRepository = processRepository;
    }

    public async Task ExecuteCreateContractAsync(Guid processId)
    {
        // Логика для создания договора
        var process = await _processRepository.GetByIdAsync(processId);
        // Логика обработки шага 1
    }

    public async Task ExecuteCalculatePaymentScheduleAsync(Guid processId)
    {
        // Логика для расчета графика платежей
        var process = await _processRepository.GetByIdAsync(processId);
        // Логика обработки шага 2
    }

    public async Task ExecuteCalculateIndebtednessAsync(Guid processId)
    {
        // Логика для расчета задолженности
        var process = await _processRepository.GetByIdAsync(processId);
        // Логика обработки шага 3
    }

    public async Task ExecuteSendContractToClientAsync(Guid processId)
    {
        // Логика для отправки контракта клиенту
        var process = await _processRepository.GetByIdAsync(processId);
        // Логика обработки шага 4
    }

    public async Task ExecuteActivateLoanAsync(Guid processId)
    {
        // Логика для активации кредита
        var process = await _processRepository.GetByIdAsync(processId);
        // Логика обработки шага 5
    }
}

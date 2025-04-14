namespace Loans.Servicing.Process;

public interface IStepExecutor
{
    Task ExecuteCreateContractAsync();
    Task ExecuteCalculatePaymentScheduleAsync();
    Task ExecuteCalculateIndebtednessAsync();
    Task ExecuteSendContractToClientAsync();
    Task ExecuteActivateLoanAsync();
}
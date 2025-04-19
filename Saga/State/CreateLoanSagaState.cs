using MassTransit;

namespace Loans.Servicing.Saga.State;

public class CreateLoanSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; } 
    public Guid ApplicationId { get; set; }
    public Guid? ContractId { get; set; }
    public string CurrentState { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

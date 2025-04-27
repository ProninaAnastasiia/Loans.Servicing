using MassTransit;

namespace Loans.Servicing.StateMachines;

public class LoanContractState : SagaStateMachineInstance 
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }

    public string Value { get; set; }
}

namespace Loans.Servicing.StateMachines.Messages;

public record CreateContractRequestedEvent
{
    public Guid CorrelationId { get; init; }
    public string Value { get; init; }
}
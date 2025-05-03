namespace Loans.Servicing.Kafka.Events.GetContractApproved;

public record ContractDetailsRequestedEvent(Guid ContractId, Guid OperationId) : EventBase;
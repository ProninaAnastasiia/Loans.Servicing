namespace Loans.Servicing.Kafka.Events.GetContractApproved;

public record ContractStatusUpdatedEvent(Guid ContractId, string Status, Guid OperationId) : EventBase;
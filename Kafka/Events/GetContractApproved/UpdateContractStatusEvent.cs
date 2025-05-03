namespace Loans.Servicing.Kafka.Events.GetContractApproved;

public record UpdateContractStatusEvent(Guid ContractId, string Status, Guid OperationId) : EventBase;
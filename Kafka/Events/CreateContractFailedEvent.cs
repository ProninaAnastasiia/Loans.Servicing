namespace Loans.Servicing.Kafka.Events;

public record CreateContractFailedEvent(Guid OperationId, string Error, string InnerError) : EventBase;
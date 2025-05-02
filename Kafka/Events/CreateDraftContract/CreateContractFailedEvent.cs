namespace Loans.Servicing.Kafka.Events.CreateDraftContract;

public record CreateContractFailedEvent(Guid OperationId, string Error, string InnerError) : EventBase;
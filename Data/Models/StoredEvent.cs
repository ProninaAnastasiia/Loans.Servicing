namespace Loans.Servicing.Data.Models;

public class StoredEvent
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Уникальный ID записи (не обязательно совпадает с EventId)
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid OperationId { get; set; }
    public DateTime OccurredOn { get; set; }
    public string Payload { get; set; }
}
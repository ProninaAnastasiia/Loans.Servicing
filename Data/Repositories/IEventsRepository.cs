using Loans.Servicing.Kafka;

namespace Loans.Servicing.Data.Repositories;

public interface IEventsRepository
{
    Task SaveAsync(EventBase @event, Guid correlationId, Guid operationId, CancellationToken cancellationToken);
    Task<List<EventBase>> GetEventsAsync(Guid correlationId, Guid operationId, CancellationToken cancellationToken);
}
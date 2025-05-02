using Loans.Servicing.Data.Models;
using Loans.Servicing.Kafka;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Loans.Servicing.Data.Repositories;

public class EventsRepository : IEventsRepository
{
    private readonly OperationsDbContext _dbContext;
    private readonly ILogger<EventsRepository> _logger;

    public EventsRepository(OperationsDbContext dbContext, ILogger<EventsRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task SaveAsync(EventBase @event, Guid correlationId, Guid operationId, CancellationToken cancellationToken)
    {
        try
        {
            var stored = new StoredEvent
            {
                EventId = @event.EventId,
                EventType = @event.EventType,
                CorrelationId = correlationId,
                OperationId = operationId,
                OccurredOn = @event.OccurredOn,
                Payload = JsonConvert.SerializeObject(@event)
            };

            _dbContext.Events.Add(stored);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении события в базу.");
            throw;
        }
    }
    
    public async Task<List<EventBase>> GetEventsAsync(Guid correlationId, Guid operationId, CancellationToken cancellationToken)
    {
        var storedEvents = await _dbContext.Events
            .Where(e => e.CorrelationId == correlationId && e.OperationId == operationId)
            .OrderBy(e => e.OccurredOn)
            .ToListAsync(cancellationToken);

        var events = new List<EventBase>();

        foreach (var stored in storedEvents)
        {
            var type = Type.GetType(stored.EventType);
            if (type == null) continue;

            var @event = (EventBase)JsonConvert.DeserializeObject(stored.Payload, type);
            events.Add(@event);
        }

        return events;
    }
    
}
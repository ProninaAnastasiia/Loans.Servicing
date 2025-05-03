using Hangfire;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka;
using Newtonsoft.Json;

namespace Loans.Servicing.Services;

public class DelayedTaskScheduler : IDelayedTaskScheduler
{
    private readonly ILogger<DelayedTaskScheduler> _logger;
    private readonly KafkaProducerService _producer;
    private readonly IEventsRepository _eventsRepository;

    public DelayedTaskScheduler(ILogger<DelayedTaskScheduler> logger, KafkaProducerService producer, IEventsRepository eventsRepository)
    {
        _logger = logger;
        _producer = producer;
        _eventsRepository = eventsRepository;
    }
    
    public Task ScheduleTaskAsync(EventBase @event, string topic, Guid correlationId, Guid operationId, TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            BackgroundJob.Schedule(() =>
                PublishEventAsync(@event, correlationId, operationId, topic, cancellationToken), delay);
            _logger.LogInformation("Task scheduled. Event: {Event}, Topic: {Topic}", @event, topic);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to schedule task: {@event.GetType().Name}, EventId: {@event.EventId}, OperationId: {operationId}", ex);
        }
    }

    // Метод, вызываемый Hangfire (должен быть публичным и без DI-параметров напрямую)
    public async Task PublishEventAsync(EventBase @event, Guid correlationId, Guid operationId, string topic, CancellationToken cancellationToken)
    {
        var jsonMessage = JsonConvert.SerializeObject(@event);
        await _producer.PublishAsync(topic, jsonMessage);
        await _eventsRepository.SaveAsync(@event, correlationId, operationId, cancellationToken);
    }
}
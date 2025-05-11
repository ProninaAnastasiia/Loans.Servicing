using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Loans.Servicing.Services;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CalculateScheduleConsumer : KafkaBackgroundConsumer
{
    public CalculateScheduleConsumer(
        IConfiguration config,
        IHandlerDispatcher handlerDispatcher,
        IServiceProvider serviceProvider,
        ILogger<CalculateScheduleConsumer> logger)
        : base(config, serviceProvider, handlerDispatcher, logger,
            topic: config["Kafka:Topics:CalculateSchedule"],
            groupId: "orchestrator-service-group",
            consumerName: nameof(CalculateScheduleConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("CalculateScheduleRequested") == true)
        {
            var @event = message.ToObject<CalculateScheduleRequested>();
            if (@event != null) await ProcessCalculateScheduleRequestedAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("ContractScheduleCalculatedEvent") == true)
        {
            var @event = message.ToObject<ContractScheduleCalculatedEvent>();
            if (@event != null) await ProcessContractScheduleCalculatedEventAsync(@event, cancellationToken);
        }
    }
    
    private async Task ProcessCalculateScheduleRequestedAsync(CalculateScheduleRequested @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события CalculateScheduleRequest: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }

    private async Task ProcessContractScheduleCalculatedEventAsync(ContractScheduleCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractScheduleCalculatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}
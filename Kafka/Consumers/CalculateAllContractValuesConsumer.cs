using Loans.Servicing.Kafka.Events.CalculateAllContractValues;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Loans.Servicing.Services;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CalculateAllContractValuesConsumer : KafkaBackgroundConsumer
{
    public CalculateAllContractValuesConsumer(
        IConfiguration config,
        IHandlerDispatcher handlerDispatcher,
        IServiceProvider serviceProvider,
        ILogger<CalculateAllContractValuesConsumer> logger)
        : base(config, serviceProvider, handlerDispatcher, logger,
            topic: config["Kafka:Topics:CalculateAllContractValues"],
            groupId: "orchestrator-service-group",
            consumerName: nameof(CalculateAllContractValuesConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("CalculateAllValuesRequested") == true)
        {
            var @event = message.ToObject<CalculateAllValuesRequested>();
            if (@event != null) await CalculateAllValuesRequestedAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("AllValuesCalculatedEvent") == true)
        {
            var @event = message.ToObject<AllValuesCalculatedEvent>();
            if (@event != null) await AllValuesCalculatedEventAsync(@event, cancellationToken);
        }
    }
    
    private async Task CalculateAllValuesRequestedAsync(CalculateAllValuesRequested @event, CancellationToken cancellationToken)
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

    private async Task AllValuesCalculatedEventAsync(AllValuesCalculatedEvent @event, CancellationToken cancellationToken)
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
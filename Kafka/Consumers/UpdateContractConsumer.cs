using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Services;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class UpdateContractConsumer : KafkaBackgroundConsumer
{
    public UpdateContractConsumer(
        IConfiguration config,
        IHandlerDispatcher handlerDispatcher,
        IServiceProvider serviceProvider,
        ILogger<CalculateContractValuesConsumer> logger)
        : base(config, serviceProvider, handlerDispatcher, logger,
            topic: config["Kafka:Topics:UpdateContractRequested"],
            groupId: "orchestrator-service-group",
            consumerName: nameof(UpdateContractConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("ContractScheduleUpdatedEvent") == true)
        {
            var @event = message.ToObject<ContractScheduleUpdatedEvent>();
            if (@event != null) await ProcessEventAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("ContractValuesUpdatedEvent") == true)
        {
            var @event = message.ToObject<ContractValuesUpdatedEvent>();
            if (@event != null) await ProcessEventAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("ContractStatusUpdatedEvent") == true)
        {
            var @event = message.ToObject<ContractStatusUpdatedEvent>();
            if (@event != null) await ProcessContractStatusUpdatedEventAsync(@event, cancellationToken);
        }
    }
    
    private async Task ProcessContractStatusUpdatedEventAsync(ContractStatusUpdatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractStatusUpdatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
    private async Task ProcessEventAsync(EventBase @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractValuesUpdatedEvent или ContractScheduleUpdatedEvent: {EventId}", @event.EventId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}
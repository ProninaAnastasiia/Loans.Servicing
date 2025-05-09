using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CalculateContractValuesConsumer : KafkaBackgroundConsumer
{
    public CalculateContractValuesConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<CalculateContractValuesConsumer> logger)
        : base(config, serviceProvider, logger,
            topic: config["Kafka:Topics:CalculateContractValues"],
            groupId: "orchestrator-service-group",
            consumerName: nameof(CalculateContractValuesConsumer)) { }
    
    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("ContractValuesCalculatedEvent") == true)
        {
            var @event = message.ToObject<ContractValuesCalculatedEvent>();
            if (@event != null) await ProcessContractValuesCalculatedEventAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("ContractScheduleCalculatedEvent") == true)
        {
            var @event = message.ToObject<ContractScheduleCalculatedEvent>();
            if (@event != null)
            {
                MetricsRegistry.StopScheduleLatencyTimer(@event.OperationId);
                await ProcessContractScheduleCalculatedEventAsync(@event, cancellationToken);
            }
        }
    }


    
    private async Task ProcessContractValuesCalculatedEventAsync(ContractValuesCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.ContractId, @event.OperationId, cancellationToken);
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<ContractValuesCalculatedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractValuesCalculatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
    private async Task ProcessContractScheduleCalculatedEventAsync(ContractScheduleCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.ContractId, @event.OperationId, cancellationToken);
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<ContractScheduleCalculatedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractScheduleCalculatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}
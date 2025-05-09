using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Services;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class UpdateContractConsumer : KafkaBackgroundConsumer
{
    public UpdateContractConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<UpdateContractConsumer> logger)
        : base(config, serviceProvider, logger,
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
            using var scope = ServiceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
            /*var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<ContractStatusUpdatedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);*/
            _logger.LogInformation("Статус контракта изменен.");
            MetricsRegistry.StopTimer(@event.OperationId); // <-- СТОП
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractStatusUpdatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
    private async Task ProcessEventAsync(EventBase @event, CancellationToken cancellationToken)
    {
        
        using var scope = ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
        var taskScheduler = scope.ServiceProvider.GetRequiredService<IDelayedTaskScheduler>();
        try
        {
            Guid contractId;
            Guid operationId;
            switch (@event)
            {
                case ContractValuesUpdatedEvent valuesEvent:
                    contractId = valuesEvent.ContractId;
                    operationId = valuesEvent.OperationId;
                    break;

                case ContractScheduleUpdatedEvent scheduleEvent:
                    contractId = scheduleEvent.ContractId;
                    operationId = scheduleEvent.OperationId;
                    break;

                default:
                    return;
            }
            
            await repository.SaveAsync(@event, contractId, operationId, cancellationToken);

            // Получаем все события для этого контракта и операции
            var events = await repository.GetEventsAsync(contractId, operationId, cancellationToken);

            // Проверяем, есть ли оба типа событий
            var valuesEventCheck = events.OfType<ContractValuesUpdatedEvent>().FirstOrDefault();
            var scheduleEventCheck = events.OfType<ContractScheduleUpdatedEvent>().FirstOrDefault();

            if (valuesEventCheck != null && scheduleEventCheck != null)
            {
                // Договор готов к подписанию. Надо завести задачу в плане операций, чтобы отправить договор клиенту
                var newEvent = new ContractDetailsRequestedEvent(contractId, operationId);
                await taskScheduler.ScheduleTaskAsync(newEvent, "update-contract-requested", contractId, operationId, TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события: {EventId}, {EventType}", @event.EventId, @event.EventType);
        }
    }
}
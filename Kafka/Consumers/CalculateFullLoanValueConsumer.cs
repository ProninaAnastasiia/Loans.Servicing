using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Loans.Servicing.Services;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CalculateFullLoanValueConsumer : KafkaBackgroundConsumer
{
    public CalculateFullLoanValueConsumer(
        IConfiguration config,
        IHandlerDispatcher handlerDispatcher,
        IServiceProvider serviceProvider,
        ILogger<CalculateFullLoanValueConsumer> logger)
        : base(config, serviceProvider, handlerDispatcher, logger,
            topic: config["Kafka:Topics:CalculateIndebtedness"],
            groupId: "orchestrator-service-group",
            consumerName: nameof(CalculateFullLoanValueConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("CalculateFullLoanValueRequested") == true)
        {
            var @event = message.ToObject<CalculateFullLoanValueRequested>();
            if (@event != null) await ProcessCalculateFullLoanValueRequestedAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("FullLoanValueCalculatedEvent") == true)
        {
            var @event = message.ToObject<FullLoanValueCalculatedEvent>();
            if (@event != null) await ProcessFullLoanValueCalculatedEventAsync(@event, cancellationToken);
        }
    }
    
    private async Task ProcessCalculateFullLoanValueRequestedAsync(CalculateFullLoanValueRequested @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события CalculateFullLoanValueRequested: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }

    private async Task ProcessFullLoanValueCalculatedEventAsync(FullLoanValueCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события FullLoanValueCalculatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}
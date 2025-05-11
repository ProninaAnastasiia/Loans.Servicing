using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Loans.Servicing.Services;

namespace Loans.Servicing.Kafka.Consumers;
using Newtonsoft.Json.Linq;

public class CreateContractConsumer : KafkaBackgroundConsumer
{
    public CreateContractConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        IHandlerDispatcher handlerDispatcher,
        ILogger<CreateContractConsumer> logger)
        : base(config, serviceProvider, handlerDispatcher, logger,
              topic: config["Kafka:Topics:CreateContractRequested"],
              groupId: "orchestrator-service-group",
              consumerName: nameof(CreateContractConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("LoanApplicationRecieved") == true)
        {
            var @event = message.ToObject<LoanApplicationRecieved>();
            if (@event != null) await ProcessLoanApplicationRecievedAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("DraftContractCreatedEvent") == true)
        {
            var @event = message.ToObject<DraftContractCreatedEvent>();
            if (@event != null) await ProcessDraftContractCreatedEventAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("CreateContractFailedEvent") == true)
        {
            var @event = message.ToObject<CreateContractFailedEvent>();
            if (@event != null) await ProcessCreateContractFailedEventAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("ContractDetailsResponseEvent") == true)
        {
            var @event = message.ToObject<ContractDetailsResponseEvent>();
            if (@event != null) await ProcessContractDetailsResponseEventAsync(@event, cancellationToken);
        }
        else if (eventType?.Contains("ContractSentToClientEvent") == true)
        {
            var @event = message.ToObject<ContractSentToClientEvent>();
            if (@event != null) await ProcessContractSentToClientEventAsync(@event, cancellationToken);
        }
    }

    private async Task ProcessLoanApplicationRecievedAsync(LoanApplicationRecieved @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события LoanApplicationRecieved: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }

    private async Task ProcessDraftContractCreatedEventAsync(DraftContractCreatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события LoanApplicationRecieved: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
    private async Task ProcessCreateContractFailedEventAsync(CreateContractFailedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractDetailsResponseEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    private async Task ProcessContractDetailsResponseEventAsync(ContractDetailsResponseEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractDetailsResponseEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    private async Task ProcessContractSentToClientEventAsync(ContractSentToClientEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await HandlerDispatcher.DispatchAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractSentToClientEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
}

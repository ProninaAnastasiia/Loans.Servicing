using Confluent.Kafka;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CreateContractConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateContractConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CreateContractConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<CreateContractConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken); // дать приложению прогрузиться
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = "orchestrator-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(_configuration["Kafka:Topics:CreateContractRequested"]);

        _logger.LogInformation("KafkaConsumerService CreateContractConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                var jsonObject = JObject.Parse(result.Message.Value);

                if (jsonObject.Property("EventType").Value.ToString().Contains("DraftContractCreatedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<DraftContractCreatedEvent>();
                    if (@event != null) await ProcessDraftContractCreatedEventAsync(@event, stoppingToken);
                }
                else if (jsonObject.Property("EventType").Value.ToString().Contains("CreateContractFailedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<CreateContractFailedEvent>();
                    if (@event != null) await ProcessCreateContractFailedEventAsync(@event, stoppingToken);
                }
                else if (jsonObject.Property("EventType").Value.ToString().Contains("ContractDetailsResponseEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractDetailsResponseEvent>();
                    if (@event != null) await ProcessContractDetailsResponseEventAsync(@event, stoppingToken);
                }
                else if (jsonObject.Property("EventType").Value.ToString().Contains("ContractSentToClientEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractSentToClientEvent>();
                    if (@event != null) await ProcessContractSentToClientEventAsync(@event, stoppingToken);
                }
            }
        }
        catch (KafkaException ex)
        {
            _logger.LogError(ex, "Kafka временно недоступна или ошибка получения сообщения.");
            await Task.Delay(1000, stoppingToken); // Ждем и пытаемся снова
        }
        finally
        {
            consumer.Close();
        }
    }
    
    private async Task ProcessDraftContractCreatedEventAsync(DraftContractCreatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.ContractId, @event.OperationId, cancellationToken);
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<DraftContractCreatedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события DraftContractCreatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
    private async Task ProcessCreateContractFailedEventAsync(CreateContractFailedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CreateContractFailedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события CreateContractFailedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    private async Task ProcessContractDetailsResponseEventAsync(ContractDetailsResponseEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<ContractDetailsResponseEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
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
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            await repository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<ContractSentToClientEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события ContractSentToClientEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}
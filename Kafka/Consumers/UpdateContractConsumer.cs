using Confluent.Kafka;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Kafka.Handlers;
using Loans.Servicing.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class UpdateContractConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdateContractConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    public UpdateContractConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<UpdateContractConsumer> logger)
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
        consumer.Subscribe(_configuration["Kafka:Topics:UpdateContractRequested"]);

        _logger.LogInformation("KafkaConsumerService UpdateContractConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                var jsonObject = JObject.Parse(result.Message.Value);

                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractScheduleUpdatedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractScheduleUpdatedEvent>();
                    if (@event != null) await ProcessEventAsync(@event, stoppingToken);
                }
                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractValuesUpdatedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractValuesUpdatedEvent>();
                    if (@event != null) await ProcessEventAsync(@event, stoppingToken);
                }
                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractStatusUpdatedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractStatusUpdatedEvent>();
                    if (@event != null) await ProcessContractStatusUpdatedEventAsync(@event, stoppingToken);
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
    
    private async Task ProcessContractStatusUpdatedEventAsync(ContractStatusUpdatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
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
        
        using var scope = _serviceProvider.CreateScope();
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
                    _logger.LogError("Неподдерживаемый тип события: {Type}", @event.GetType().FullName);
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
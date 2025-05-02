using Confluent.Kafka;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CalculateContractValuesConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalculateContractValuesConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CalculateContractValuesConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<CalculateContractValuesConsumer> logger)
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
        consumer.Subscribe(_configuration["Kafka:Topics:CalculateContractValues"]);

        _logger.LogInformation("KafkaConsumerService CalculateContractValuesConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                var jsonObject = JObject.Parse(result.Message.Value);

                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractValuesCalculatedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractValuesCalculatedEvent>();
                    if (@event != null) await ProcessContractValuesCalculatedEventAsync(@event, stoppingToken);
                }
                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractScheduleCalculatedEvent"))
                {
                    _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);
                    var @event = jsonObject.ToObject<ContractScheduleCalculatedEvent>();
                    if (@event != null) await ProcessContractScheduleCalculatedEventAsync(@event, stoppingToken);
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
    
    private async Task ProcessContractValuesCalculatedEventAsync(ContractValuesCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
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
            using var scope = _serviceProvider.CreateScope();
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
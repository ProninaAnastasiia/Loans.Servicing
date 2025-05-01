using Confluent.Kafka;
using Loans.Servicing.Kafka.Events;
using Loans.Servicing.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CalculateIndebtednessConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalculateIndebtednessConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CalculateIndebtednessConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<CalculateIndebtednessConsumer> logger)
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
        consumer.Subscribe(_configuration["Kafka:Topics:CalculateIndebtedness"]);

        _logger.LogInformation("KafkaConsumerService CalculateIndebtednessConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);

                var jsonObject = JObject.Parse(result.Message.Value);

                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractValuesCalculatedEvent"))
                {
                    var @event = jsonObject.ToObject<ContractValuesCalculatedEvent>();
                    if (@event != null) await ProcessContractValuesCalculatedEventAsync(@event, stoppingToken);
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
            _logger.LogError(ex, "Ошибка при обработке события RepaymentScheduleCalculatedEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
}
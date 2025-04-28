using Confluent.Kafka;
using Loans.Servicing.Kafka.Events;
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

                _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);

                var jsonObject = JObject.Parse(result.Message.Value);

                // Определяем тип события по наличию определенных свойств
                if (jsonObject.Property("EventType").Value.ToString().Contains("ContractScheduleUpdatedEvent"))
                {
                    var @event = jsonObject.ToObject<ContractScheduleUpdatedEvent>();
                    if (@event != null) await ProcessUpdateContractScheduleEventAsync(@event, stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Неизвестный тип события: {Json}", result.Message.Value);
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
    
    private async Task ProcessUpdateContractScheduleEventAsync(ContractScheduleUpdatedEvent updatedEvent, CancellationToken cancellationToken)
    {
        try
        {
            /*using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<UpdateContractScheduleEvent>>();
            await handler.HandleAsync(@event, cancellationToken);*/
            _logger.LogInformation("Сервис ссуд успешно обновил ScheduleId");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события RepaymentScheduleCalculatedEvent: {EventId}, {OperationId}", updatedEvent.EventId, updatedEvent.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
}
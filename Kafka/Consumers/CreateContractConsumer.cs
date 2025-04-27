using System.Threading.Channels;
using Confluent.Kafka;
using Loans.Servicing.Kafka.Events;
using Loans.Servicing.Kafka.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public class CreateContractConsumer: BackgroundService
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
            GroupId = "contract-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(_configuration["Kafka:Topics:CreateContractResult"]);

        _logger.LogInformation("KafkaConsumerService CreateContractConsumer запущен.");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                _logger.LogInformation("Получено сообщение из Kafka: {Message}", result.Message.Value);

                var jsonObject = JObject.Parse(result.Message.Value);

                // Определяем тип события по наличию определенных свойств
                if (jsonObject.Property("EventType").Value.ToString().Contains("DraftContractCreatedEvent"))
                {
                    var @event = jsonObject.ToObject<DraftContractCreatedEvent>();
                    if (@event != null) await ProcessDraftContractCreatedEventAsync(@event, stoppingToken);
                }
                else if (jsonObject.Property("EventType").Value.ToString().Contains("CreateContractFailedEvent"))
                {
                    var @event = jsonObject.ToObject<CreateContractFailedEvent>();
                    if (@event != null) await ProcessCreateContractFailedEventAsync(@event, stoppingToken);
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
    
    private async Task ProcessDraftContractCreatedEventAsync(DraftContractCreatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<DraftContractCreatedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
    
    private async Task ProcessCreateContractFailedEventAsync(CreateContractFailedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CreateContractFailedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }

    
}
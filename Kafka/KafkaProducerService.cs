using Confluent.Kafka;

namespace Loans.Servicing.Kafka;

public class KafkaProducerService
{
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _configuration = configuration;

        var producerconfig = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            Acks = Acks.All, // гарантирует доставку в реплицированные брокеры
            MessageTimeoutMs = 5000,
            EnableIdempotence = true // защита от дублирующей отправки при сбоях
        };

        _logger = logger;
        _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
    }

    public async Task PublishAsync(string topic, string message)
    {
        var kafkamessage = new Message<Null, string> { Value = message, };

        try
        {
            DeliveryResult<Null, string> result = await _producer.ProduceAsync(topic, kafkamessage);

            _logger.LogInformation("Опубликовано сообщение в Kafka.  Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Message: {Message}",
                topic, result.Partition, result.Offset, message);
        }
        catch (ProduceException<Null, string> ex)
        {
            // Обработка исключения, специфичного для Kafka Producer
            _logger.LogError(ex, "Ошибка при публикации сообщения в Kafka.  Topic: {Topic}, Message: {Message}, Status: {Status}",
                topic, message, ex.Error.Code);
        }
        catch (Exception ex)
        {
            // Обработка общих исключений
            _logger.LogError(ex, "Неизвестная ошибка при публикации сообщения в Kafka. Topic: {Topic}, Message: {Message}", topic, message);
        }
    }
}
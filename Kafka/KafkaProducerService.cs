using Confluent.Kafka;
using Prometheus;

namespace Loans.Servicing.Kafka;

public class KafkaProducerService
{
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;
    
    private static readonly Counter KafkaMessagesPublished = Metrics.CreateCounter("kafka_messages_published_total", "Total number of messages published to Kafka");
    private static readonly Counter KafkaMessagesFailed = Metrics.CreateCounter("kafka_messages_failed_total", "Total number of failed Kafka message publishes");
    
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
            KafkaMessagesPublished.Inc();
        }
        catch (ProduceException<Null, string> ex)
        {
            // Обработка исключения, специфичного для Kafka Producer
            _logger.LogError(ex, "Ошибка при публикации сообщения в Kafka.  Topic: {Topic}, Message: {Message}, Status: {Status}", topic, message, ex.Error.Code);
            KafkaMessagesFailed.Inc();
        }
        catch (Exception ex)
        {
            // Обработка общих исключений
            _logger.LogError(ex, "Неизвестная ошибка при публикации сообщения в Kafka. Topic: {Topic}, Message: {Message}", topic, message);
            KafkaMessagesFailed.Inc();
        }
    }
}
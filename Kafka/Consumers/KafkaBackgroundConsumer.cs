using Confluent.Kafka;
using Loans.Servicing.Services;
using Newtonsoft.Json.Linq;

namespace Loans.Servicing.Kafka.Consumers;

public abstract class KafkaBackgroundConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    protected readonly ILogger _logger;
    private readonly string _topic;
    private readonly string _groupId;
    private readonly int _parallelism;
    private readonly string _consumerName;
    protected IServiceProvider ServiceProvider { get; }
    protected IHandlerDispatcher HandlerDispatcher { get; }

    protected KafkaBackgroundConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IHandlerDispatcher handlerDispatcher,
        ILogger logger,
        string topic,
        string groupId,
        string consumerName,
        int parallelism = 10)
    {
        _configuration = configuration;
        
        ServiceProvider = serviceProvider;
        HandlerDispatcher = handlerDispatcher;
        _logger = logger;
        _topic = topic;
        _groupId = groupId;
        _parallelism = parallelism;
        _consumerName = consumerName;
    }

    protected abstract Task HandleMessageAsync(JObject message, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken); // Дать приложению прогрузиться

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer {ConsumerName} запущен и слушает топик {Topic}", _consumerName, _topic);

        var semaphore = new SemaphoreSlim(_parallelism);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                await semaphore.WaitAsync(stoppingToken);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var json = JObject.Parse(result.Message.Value);
                        await HandleMessageAsync(json, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{ConsumerName}] Ошибка при обработке сообщения: {Message}", _consumerName, result.Message.Value);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
        catch (KafkaException ex)
        {
            _logger.LogError(ex, "[{ConsumerName}] Kafka ошибка.", _consumerName);
        }
        finally
        {
            consumer.Close();
        }
    }
}

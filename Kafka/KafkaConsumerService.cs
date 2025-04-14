using System.Text.Json;
using Confluent.Kafka;
using Loans.Servicing.Kafka.Messages;
using Loans.Servicing.Process;

namespace Loans.Servicing.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly StepExecutor _executor;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(StepExecutor executor, ILogger<KafkaConsumerService> logger)
    {
        _executor = executor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new ConsumerBuilder<Ignore, string>(
            new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "orchestrator-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();

        consumer.Subscribe("process-steps");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var message = result?.Message?.Value;

                if (!string.IsNullOrWhiteSpace(message))
                {
                    var data = JsonSerializer.Deserialize<StepTriggerMessage>(message);

                    if (data != null)
                    {
                        _logger.LogInformation($"Processing Step {data.StepIndex} for Process {data.ProcessId}");
                        
                        // Делаем делегирование на соответствующий метод в зависимости от шага
                        switch (data.StepIndex)
                        {
                            case 1:
                                await _executor.ExecuteCreateContractAsync(data.ProcessId);
                                break;
                            case 2:
                                await _executor.ExecuteCalculatePaymentScheduleAsync(data.ProcessId);
                                break;
                            case 3:
                                await _executor.ExecuteCalculateIndebtednessAsync(data.ProcessId);
                                break;
                            case 4:
                                await _executor.ExecuteSendContractToClientAsync(data.ProcessId);
                                break;
                            case 5:
                                await _executor.ExecuteActivateLoanAsync(data.ProcessId);
                                break;
                            default:
                                _logger.LogWarning($"Unknown StepIndex {data.StepIndex} for Process {data.ProcessId}");
                                break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                _logger.LogInformation("Kafka Consumer shutdown requested.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"KafkaConsumer error: {ex.Message}");
            }
        }

        consumer.Close(); // Закрываем consumer по окончании
    }
}


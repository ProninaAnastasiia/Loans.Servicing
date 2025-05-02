using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class FullLoanValueCalculatedHandler : IEventHandler<FullLoanValueCalculatedEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<FullLoanValueCalculatedHandler> _logger;
    private readonly KafkaProducerService _producer;

    public FullLoanValueCalculatedHandler(ILogger<FullLoanValueCalculatedHandler> logger, IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _config = config;
        _producer = producer;
    }

    public async Task HandleAsync(FullLoanValueCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];

            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle FullLoanValueCalculatedEvent. OperationId: {OperationId}. Exception: {e}", @event.OperationId, e.Message);
        }
    }
}
using Loans.Servicing.Kafka.Events;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class ContractValuesCalculatedHandler : IEventHandler<ContractValuesCalculatedEvent>
{
    private readonly ILogger<ContractValuesCalculatedHandler> _logger;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public ContractValuesCalculatedHandler(ILogger<ContractValuesCalculatedHandler> logger, IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _config = config;
        _producer = producer;
    }

    public async Task HandleAsync(ContractValuesCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];

            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle ContractValuesCalculatedEvent. OperationId: {OperationId}. Exception: {e}", @event.OperationId, e.Message);
        }
    }
}
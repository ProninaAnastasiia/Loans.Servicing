using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class ContractScheduleCalculatedHandler : IEventHandler<ContractScheduleCalculatedEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ContractScheduleCalculatedHandler> _logger;
    private readonly KafkaProducerService _producer;

    public ContractScheduleCalculatedHandler(ILogger<ContractScheduleCalculatedHandler> logger, IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _config = config;
        _producer = producer;
    }

    public async Task HandleAsync(ContractScheduleCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];

            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle ContractScheduleCalculatedEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}", @event.ContractId , @event.OperationId, e.Message);
        }
    }
}
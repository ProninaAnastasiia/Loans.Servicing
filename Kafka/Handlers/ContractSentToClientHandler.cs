using Loans.Servicing.Kafka.Events.GetContractApproved;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class ContractSentToClientHandler : IEventHandler<ContractSentToClientEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ContractSentToClientHandler> _logger;
    private readonly KafkaProducerService _producer;

    public ContractSentToClientHandler(ILogger<ContractSentToClientHandler> logger, IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _config = config;
        _producer = producer;
    }

    public async Task HandleAsync(ContractSentToClientEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var newEvent = new UpdateContractStatusEvent(@event.ContractId, "Подписан", @event.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(newEvent);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];

            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle ContractScheduleCalculatedEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}", @event.ContractId , @event.OperationId, e.Message);
        }
    }
}
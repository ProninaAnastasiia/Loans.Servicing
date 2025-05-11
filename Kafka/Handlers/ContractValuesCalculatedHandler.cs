using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class ContractValuesCalculatedHandler : IEventHandler<ContractValuesCalculatedEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ContractValuesCalculatedHandler> _logger;
    private readonly KafkaProducerService _producer;
    private readonly IEventsRepository _eventsRepository;

    public ContractValuesCalculatedHandler(ILogger<ContractValuesCalculatedHandler> logger, IConfiguration config, KafkaProducerService producer, IEventsRepository eventsRepository)
    {
        _logger = logger;
        _config = config;
        _producer = producer;
        _eventsRepository = eventsRepository;
    }

    public async Task HandleAsync(ContractValuesCalculatedEvent innerEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(innerEvent, innerEvent.ContractId, innerEvent.OperationId, cancellationToken);
            var jsonMessage = JsonConvert.SerializeObject(innerEvent);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];
            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle ContractValuesCalculatedEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}", innerEvent.ContractId , innerEvent.OperationId, e.Message);
        }
    }
}
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class FullLoanValueCalculatedHandler : IEventHandler<FullLoanValueCalculatedEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<FullLoanValueCalculatedHandler> _logger;
    private readonly KafkaProducerService _producer;
    private readonly IEventsRepository _eventsRepository;

    public FullLoanValueCalculatedHandler(IEventsRepository eventsRepository, ILogger<FullLoanValueCalculatedHandler> logger, IConfiguration config, KafkaProducerService producer)
    {
        _eventsRepository = eventsRepository;
        _logger = logger;
        _config = config;
        _producer = producer;
    }

    public async Task HandleAsync(FullLoanValueCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(@event, @event.ContractId, @event.OperationId, cancellationToken);
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
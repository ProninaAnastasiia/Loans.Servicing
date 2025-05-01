using Loans.Servicing.Kafka.Events;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class FullLoanValueCalculatedHandler : IEventHandler<FullLoanValueCalculatedEvent>
{
    private readonly ILogger<FullLoanValueCalculatedHandler> _logger;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
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
            //TODO: Переделать это на общее событие для расчета необходимых для договора полей
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];

            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle DraftContractCreatedEvent. OperationId: {OperationId}. Exception: {e}", @event.OperationId, e.Message);
        }
    }
}
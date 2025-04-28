using AutoMapper;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class RepaymentScheduleCalculatedHandler : IEventHandler<RepaymentScheduleCalculatedEvent>
{
    private readonly ILogger<RepaymentScheduleCalculatedHandler> _logger;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public RepaymentScheduleCalculatedHandler(ILogger<RepaymentScheduleCalculatedHandler> logger, IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _config = config;
        _producer = producer;
    }

    public async Task HandleAsync(RepaymentScheduleCalculatedEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            var jsonMessage = JsonConvert.SerializeObject(contractEvent);
            var topic = _config["Kafka:Topics:UpdateContractRequested"];

            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle DraftContractCreatedEvent. OperationId: {OperationId}. Exception: {e}", contractEvent.OperationId, e.Message);
        }
    }
}
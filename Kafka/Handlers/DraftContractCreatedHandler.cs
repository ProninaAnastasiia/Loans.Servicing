using AutoMapper;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class DraftContractCreatedHandler: IEventHandler<DraftContractCreatedEvent>
{
    private readonly IOperationRepository _operationRepository;
    private readonly ILogger<DraftContractCreatedHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public DraftContractCreatedHandler(IOperationRepository operationRepository, ILogger<DraftContractCreatedHandler> logger, IMapper mapper,IConfiguration config, KafkaProducerService producer)
    {
        _operationRepository = operationRepository;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(DraftContractCreatedEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            var operation = await _operationRepository.GetByIdAsync(contractEvent.OperationId);
            await _operationRepository.UpdateStatusAsync(operation, OperationStatus.InProgress);

            var @event1 = _mapper.Map<CalculateRepaymentScheduleEvent>(contractEvent);
            var @event2 = _mapper.Map<CalculateContractValuesEvent>(contractEvent);
            var jsonMessage1 = JsonConvert.SerializeObject(@event1);
            var jsonMessage2 = JsonConvert.SerializeObject(@event2);
            var topic1 = _config["Kafka:Topics:CalculateRepaymentSchedule"];
            var topic2 = _config["Kafka:Topics:CalculateIndebtedness"];
            
            await _producer.PublishAsync(topic1, jsonMessage1);
            await _producer.PublishAsync(topic2, jsonMessage2);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle DraftContractCreatedEvent. OperationId: {OperationId}. Exception: {e}", contractEvent.OperationId, e.Message);
        }
    }
}
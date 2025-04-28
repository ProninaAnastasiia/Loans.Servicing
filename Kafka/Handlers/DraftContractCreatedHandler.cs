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

            var @event = _mapper.Map<CalculateRepaymentScheduleEvent>(contractEvent);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateRepaymentSchedule"];
            
            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle DraftContractCreatedEvent. OperationId: {OperationId}. Exception: {e}", contractEvent.OperationId, e.Message);
        }
    }
}
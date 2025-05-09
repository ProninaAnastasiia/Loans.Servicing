using AutoMapper;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class DraftContractCreatedHandler : IEventHandler<DraftContractCreatedEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<DraftContractCreatedHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IOperationRepository _operationRepository;
    private readonly KafkaProducerService _producer;

    public DraftContractCreatedHandler(IOperationRepository operationRepository,
        ILogger<DraftContractCreatedHandler> logger, IMapper mapper, IConfiguration config, KafkaProducerService producer)
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

            var @event = _mapper.Map<CalculateContractValuesEvent>(contractEvent);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateContractValues"];

            await _producer.PublishAsync(topic, jsonMessage);
            MetricsRegistry.StartScheduleLatencyTimer(@event.OperationId);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle DraftContractCreatedEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}",contractEvent.ContractId , contractEvent.OperationId, e.Message);
        }
    }
}
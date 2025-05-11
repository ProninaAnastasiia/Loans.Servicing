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
    private readonly IEventsRepository _eventsRepository;

    public DraftContractCreatedHandler(IOperationRepository operationRepository, IEventsRepository eventsRepository,
        ILogger<DraftContractCreatedHandler> logger, IMapper mapper, IConfiguration config, KafkaProducerService producer)
    {
        _operationRepository = operationRepository;
        _eventsRepository = eventsRepository;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(DraftContractCreatedEvent innerEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(innerEvent, innerEvent.ContractId, innerEvent.OperationId, cancellationToken);
            var operation = await _operationRepository.GetByIdAsync(innerEvent.OperationId);
            await _operationRepository.UpdateStatusAsync(operation, OperationStatus.InProgress);

            var @event = _mapper.Map<CalculateContractValuesEvent>(innerEvent);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateContractValues"];

            await _producer.PublishAsync(topic, jsonMessage);
            MetricsRegistry.StartScheduleLatencyTimer(@event.OperationId);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle DraftContractCreatedEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}",innerEvent.ContractId , innerEvent.OperationId, e.Message);
        }
    }
}
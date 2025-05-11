using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateRepaymentSchedule;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class CalculateScheduleRequestedHandler : IEventHandler<CalculateScheduleRequested>
{
    private readonly IConfiguration _config;
    private readonly ILogger<CalculateScheduleRequestedHandler> _logger;
    private readonly KafkaProducerService _producer;
    private readonly IMapper _mapper;
    private readonly IOperationRepository _operationRepository;
    private readonly IEventsRepository _eventsRepository;
    
    public CalculateScheduleRequestedHandler(IMapper mapper, ILogger<CalculateScheduleRequestedHandler> logger, IConfiguration config, KafkaProducerService producer, IOperationRepository operationRepository, IEventsRepository eventsRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _config = config;
        _producer = producer;
        _operationRepository = operationRepository;
        _eventsRepository = eventsRepository;
    }

    public async Task HandleAsync(CalculateScheduleRequested innerEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(innerEvent, innerEvent.OperationId, innerEvent.OperationId, cancellationToken);
            var request = _mapper.Map<CalculateScheduleRequest>(innerEvent);
            var operation = new OperationEntity
            {
                OperationId = innerEvent.OperationId,
                Description = "Создание черновика контракта",
                Status = OperationStatus.Started,
                ContextJson = JsonConvert.SerializeObject(request),
                StartedAt = DateTime.UtcNow
            };
            await _operationRepository.SaveAsync(operation);
            var @event = _mapper.Map<CalculateRepaymentScheduleEvent>(request, opt => opt.Items["OperationId"] = innerEvent.OperationId);

            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateSchedule"];
    
            await _producer.PublishAsync(topic, jsonMessage);
    
            await _eventsRepository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateScheduleRequested. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}", innerEvent.ContractId , innerEvent.OperationId, e.Message);
        }
    }
}
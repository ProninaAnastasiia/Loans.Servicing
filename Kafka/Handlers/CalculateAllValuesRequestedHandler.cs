using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class CalculateAllValuesRequestedHandler : IEventHandler<CalculateAllValuesRequested>
{
    private readonly IConfiguration _config;
    private readonly ILogger<CalculateAllValuesRequestedHandler> _logger;
    private readonly KafkaProducerService _producer;
    private readonly IMapper _mapper;
    private readonly IOperationRepository _operationRepository;
    private readonly IEventsRepository _eventsRepository;
    
    public CalculateAllValuesRequestedHandler(IMapper mapper, ILogger<CalculateAllValuesRequestedHandler> logger, IConfiguration config, KafkaProducerService producer, IOperationRepository operationRepository, IEventsRepository eventsRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _config = config;
        _producer = producer;
        _operationRepository = operationRepository;
        _eventsRepository = eventsRepository;
    }

    public async Task HandleAsync(CalculateAllValuesRequested innerEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(innerEvent, innerEvent.OperationId, innerEvent.OperationId, cancellationToken);
            var request = _mapper.Map<CalculateAllContractValuesRequest>(innerEvent);
            var operation = new OperationEntity
            {
                OperationId = innerEvent.OperationId,
                Description = "Расчет всех необходимых контракту значений",
                Status = OperationStatus.Started,
                ContextJson = JsonConvert.SerializeObject(request),
                StartedAt = DateTime.UtcNow
            };
            await _operationRepository.SaveAsync(operation);
            var @event = _mapper.Map<CalculateContractValuesEvent>(request, opt => opt.Items["OperationId"] = innerEvent.OperationId);
            //var @event = new CalculateContractValuesEvent(request.ContractId, request.LoanAmount, request.LoanTermMonths, request.InterestRate, request.PaymentType, request.OperationId)
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateAllContractValues"];
    
            await _producer.PublishAsync(topic, jsonMessage);
    
            await _eventsRepository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateAllValuesRequested. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}", innerEvent.ContractId , innerEvent.OperationId, e.Message);
        }
    }
}
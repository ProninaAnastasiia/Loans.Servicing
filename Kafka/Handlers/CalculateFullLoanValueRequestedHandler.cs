using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;
using Loans.Servicing.Kafka.Events.CalculateRepaymentSchedule;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class CalculateFullLoanValueRequestedHandler : IEventHandler<CalculateFullLoanValueRequested>
{
    private readonly IConfiguration _config;
    private readonly ILogger<CalculateFullLoanValueRequestedHandler> _logger;
    private readonly KafkaProducerService _producer;
    private readonly IMapper _mapper;
    private readonly IOperationRepository _operationRepository;
    private readonly IEventsRepository _eventsRepository;
    
    public CalculateFullLoanValueRequestedHandler(IMapper mapper, ILogger<CalculateFullLoanValueRequestedHandler> logger, IConfiguration config, KafkaProducerService producer, IOperationRepository operationRepository, IEventsRepository eventsRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _config = config;
        _producer = producer;
        _operationRepository = operationRepository;
        _eventsRepository = eventsRepository;
    }

    public async Task HandleAsync(CalculateFullLoanValueRequested innerEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(innerEvent, innerEvent.OperationId, innerEvent.OperationId, cancellationToken);
            var request = _mapper.Map<CalculateFullLoanValueRequest>(innerEvent);
            var operation = new OperationEntity
            {
                OperationId = innerEvent.OperationId,
                Description = "Создание черновика контракта",
                Status = OperationStatus.Started,
                ContextJson = JsonConvert.SerializeObject(request),
                StartedAt = DateTime.UtcNow
            };
            await _operationRepository.SaveAsync(operation);
            var @event = _mapper.Map<CalculateFullLoanValueEvent>(request, opt => opt.Items["OperationId"] = innerEvent.OperationId);

            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateIndebtedness"];
    
            await _producer.PublishAsync(topic, jsonMessage);
    
            await _eventsRepository.SaveAsync(@event, @event.OperationId, @event.OperationId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateFullLoanValueRequested. OperationId: {OperationId}. Exception: {e}", innerEvent.OperationId, e.Message);
        }
    }
}
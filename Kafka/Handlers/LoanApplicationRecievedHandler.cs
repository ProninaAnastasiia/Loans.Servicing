using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class LoanApplicationRecievedHandler : IEventHandler<LoanApplicationRecieved>
{
    private readonly IConfiguration _config;
    private readonly ILogger<LoanApplicationRecievedHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IOperationRepository _operationRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly KafkaProducerService _producer;

    public LoanApplicationRecievedHandler(IOperationRepository operationRepository, IEventsRepository eventsRepository,
        ILogger<LoanApplicationRecievedHandler> logger, IMapper mapper, IConfiguration config, KafkaProducerService producer)
    {
        _operationRepository = operationRepository;
        _eventsRepository = eventsRepository;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(LoanApplicationRecieved applicationEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(applicationEvent, applicationEvent.OperationId, applicationEvent.OperationId, cancellationToken);
            var application = _mapper.Map<LoanApplicationRequest>(applicationEvent);
            var operation = new OperationEntity
            {
                OperationId = applicationEvent.OperationId,
                Description = "Создание черновика контракта",
                Status = OperationStatus.Started,
                ContextJson = JsonConvert.SerializeObject(application),
                StartedAt = DateTime.UtcNow
            };
            await _operationRepository.SaveAsync(operation);
            var @event = _mapper.Map<CreateContractRequestedEvent>(application, opt => opt.Items["OperationId"] = applicationEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CreateContractRequested"];
    
            await _producer.PublishAsync(topic, jsonMessage);
    
            await _eventsRepository.SaveAsync(@event, applicationEvent.OperationId, @event.OperationId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle LoanApplicationRecieved event. Exception: {e}", e.Message);
        }
    }
}
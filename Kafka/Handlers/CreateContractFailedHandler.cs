using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Newtonsoft.Json;

namespace Loans.Servicing.Kafka.Handlers;

public class CreateContractFailedHandler : IEventHandler<CreateContractFailedEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<CreateContractFailedHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IOperationRepository _operationRepository;
    private readonly IEventsRepository _eventsRepository;

    private readonly KafkaProducerService _producer;

    public CreateContractFailedHandler(IOperationRepository operationRepository, IEventsRepository eventsRepository,
        ILogger<CreateContractFailedHandler> logger, IMapper mapper, IConfiguration config, KafkaProducerService producer)
    {
        _operationRepository = operationRepository;
        _eventsRepository = eventsRepository;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CreateContractFailedEvent innerEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(innerEvent, innerEvent.OperationId, innerEvent.OperationId, cancellationToken);
            var operation = await _operationRepository.GetByIdAsync(innerEvent.OperationId);
            await _operationRepository.UpdateStatusAsync(operation, OperationStatus.Failed);

            var operationId = Guid.NewGuid();
            var newOperation = new OperationEntity
            {
                OperationId = operationId,
                Description = "Создание черновика контракта",
                Status = OperationStatus.Started,
                ContextJson = operation.ContextJson,
                StartedAt = DateTime.UtcNow
            };
            await _operationRepository.SaveAsync(newOperation);
            
            var context = JsonConvert.DeserializeObject<LoanApplicationRequest>(operation.ContextJson);
            
            var @event = _mapper.Map<CreateContractRequestedEvent>(context, opt => opt.Items["OperationId"] = operationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CreateContractRequested"];
    
            await _producer.PublishAsync(topic, jsonMessage);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CreateContractFailedEvent. OperationId: {OperationId}. Exception: {e}", innerEvent.OperationId, e.Message);

        }
    }
}
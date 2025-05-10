using AutoMapper;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Services;

namespace Loans.Servicing.Kafka.Handlers;

public class ContractDetailsResponseHandler : IEventHandler<ContractDetailsResponseEvent>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ContractDetailsResponseHandler> _logger;
    private readonly IDelayedTaskScheduler _delayedTaskScheduler;
    private readonly IMapper _mapper;
    private readonly IEventsRepository _eventsRepository;

    public ContractDetailsResponseHandler(ILogger<ContractDetailsResponseHandler> logger, IConfiguration config, IDelayedTaskScheduler delayedTaskScheduler, IMapper mapper, IEventsRepository eventsRepository)
    {
        _logger = logger;
        _config = config;
        _delayedTaskScheduler = delayedTaskScheduler;
        _mapper = mapper;
        _eventsRepository = eventsRepository;
    }

    public async Task HandleAsync(ContractDetailsResponseEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await _eventsRepository.SaveAsync(@event, @event.ContractId, @event.OperationId, cancellationToken);
            var newEvent = _mapper.Map<ContractSentToClientEvent>(@event);
            var topic = _config["Kafka:Topics:CreateContractRequested"];
            await _delayedTaskScheduler.ScheduleTaskAsync(newEvent, topic, @event.ContractId, @event.OperationId, TimeSpan.FromSeconds(5),cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle ContractScheduleCalculatedEvent. ContractId: {ContractId}, OperationId: {OperationId}. Exception: {e}", @event.ContractId , @event.OperationId, e.Message);
        }
    }
}
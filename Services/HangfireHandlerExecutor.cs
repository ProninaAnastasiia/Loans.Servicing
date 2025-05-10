using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Kafka.Handlers;

namespace Loans.Servicing.Services;

public class HangfireHandlerExecutor<TEvent> where TEvent : class
{
    private readonly IEventsRepository _eventsRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HangfireHandlerExecutor<TEvent>> _logger;

    public HangfireHandlerExecutor(IEventsRepository eventsRepository, IServiceProvider serviceProvider, ILogger<HangfireHandlerExecutor<TEvent>> logger)
    {
        _eventsRepository = eventsRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
    {
        if (@event.GetType() == typeof(ContractScheduleUpdatedEvent) || @event.GetType() == typeof(ContractValuesUpdatedEvent))
        {
            using var scope = _serviceProvider.CreateScope();
            var taskScheduler = scope.ServiceProvider.GetRequiredService<IDelayedTaskScheduler>();
            Guid contractId;
            Guid operationId;
            switch (@event)
            {
                case ContractValuesUpdatedEvent valuesEvent:
                    contractId = valuesEvent.ContractId;
                    operationId = valuesEvent.OperationId;
                    await _eventsRepository.SaveAsync(valuesEvent, contractId, operationId, cancellationToken);
                    break;
    
                case ContractScheduleUpdatedEvent scheduleEvent:
                    contractId = scheduleEvent.ContractId;
                    operationId = scheduleEvent.OperationId;
                    await _eventsRepository.SaveAsync(scheduleEvent, contractId, operationId, cancellationToken);
                    break;
    
                default:
                    return;
            }
    
            // Получаем все события для этого контракта и операции
            var events = await _eventsRepository.GetEventsAsync(contractId, operationId, cancellationToken);
    
            // Проверяем, есть ли оба типа событий
            var valuesEventCheck = events.OfType<ContractValuesUpdatedEvent>().FirstOrDefault();
            var scheduleEventCheck = events.OfType<ContractScheduleUpdatedEvent>().FirstOrDefault();
    
            if (valuesEventCheck != null && scheduleEventCheck != null)
            {
                // Договор готов к подписанию. Надо завести задачу в плане операций, чтобы отправить договор клиенту
                var newEvent = new ContractDetailsRequestedEvent(contractId, operationId);
                await taskScheduler.ScheduleTaskAsync(newEvent, "update-contract-requested", contractId, operationId, TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
        else if (@event is ContractStatusUpdatedEvent statusEvent)
        {
            await _eventsRepository.SaveAsync(statusEvent, statusEvent.ContractId, statusEvent.OperationId, cancellationToken);
            /*var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<ContractStatusUpdatedEvent>>();
            await handler.HandleAsync(@event, cancellationToken);*/
            _logger.LogInformation("Статус контракта изменен.");
            MetricsRegistry.StopTimer(statusEvent.OperationId);
        }
        else
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
            await handler.HandleAsync(@event, CancellationToken.None);
        }
    }
}

using Loans.Servicing.Kafka.Events.CalculateAllContractValues;

namespace Loans.Servicing.Kafka.Handlers;

public class AllValuesCalculatedHandler : IEventHandler<AllValuesCalculatedEvent>
{
    private readonly ILogger<AllValuesCalculatedHandler> _logger;

    public AllValuesCalculatedHandler(ILogger<AllValuesCalculatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(AllValuesCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Все необходимые контракту значения рассчитаны. OperationId: {OperationId}", @event.OperationId);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle AllValuesCalculatedEvent. OperationId: {OperationId}. Exception: {e}", @event.OperationId, e.Message);
        }
    }
}
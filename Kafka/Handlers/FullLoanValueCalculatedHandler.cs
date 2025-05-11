using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;

namespace Loans.Servicing.Kafka.Handlers;

public class FullLoanValueCalculatedHandler : IEventHandler<FullLoanValueCalculatedEvent>
{
    private readonly ILogger<FullLoanValueCalculatedHandler> _logger;

    public FullLoanValueCalculatedHandler(ILogger<FullLoanValueCalculatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(FullLoanValueCalculatedEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("ПСК рассчитано: {FullLoanValue}, OperationId: {OperationId}", @event.FullLoanValue, @event.OperationId);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle FullLoanValueCalculatedEvent. OperationId: {OperationId}. Exception: {e}", @event.OperationId, e.Message);
        }
    }
}
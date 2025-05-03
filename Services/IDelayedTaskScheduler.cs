using Loans.Servicing.Kafka;

namespace Loans.Servicing.Services;

public interface IDelayedTaskScheduler
{
    Task ScheduleTaskAsync(EventBase @event, string topic, Guid correlationId, Guid operationId, TimeSpan delay, CancellationToken cancellationToken);
}
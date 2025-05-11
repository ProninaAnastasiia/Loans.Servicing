namespace Loans.Servicing.Kafka.Handlers;

public interface IEventHandler<T>
{
    Task HandleAsync(T innerEvent, CancellationToken cancellationToken);
}
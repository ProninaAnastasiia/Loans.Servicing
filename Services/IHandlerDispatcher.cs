namespace Loans.Servicing.Services;

public interface IHandlerDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class;
}
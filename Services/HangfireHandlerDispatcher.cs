using Hangfire;

namespace Loans.Servicing.Services;

public class HangfireHandlerDispatcher : IHandlerDispatcher
{
    public Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            BackgroundJob.Enqueue<HangfireHandlerExecutor<TEvent>>(x => x.ExecuteAsync(@event, cancellationToken));
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to dispatch event {typeof(TEvent).Name}", ex);
        }
    }
}
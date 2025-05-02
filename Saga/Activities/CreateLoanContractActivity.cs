using Loans.Servicing.Kafka.Events;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Saga.Commands;
using Loans.Servicing.Saga.State;
using MassTransit;

namespace Loans.Servicing.Saga.Activities;
// Ссыль на доку: https://masstransit.io/documentation/configuration/sagas/custom
public class CreateLoanContractActivity : IStateMachineActivity<CreateLoanSagaState, CreateContractRequestedEvent>
{
    private IStateMachineActivity<CreateLoanSagaState> _stateMachineActivityImplementation;

    public async Task Execute(BehaviorContext<CreateLoanSagaState, CreateContractRequestedEvent> context, IBehavior<CreateLoanSagaState, CreateContractRequestedEvent> next)
    {
        var endpoint = await context.GetSendEndpoint(new Uri("queue:loans-contracts-create-draft"));
        await endpoint.Send(new CreateDraftContractCommand
        {
            ApplicationId = context.Data.ApplicationId,
            // остальные поля
        });

        // always call the next activity in the behavior
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateLoanSagaState, CreateContractRequestedEvent, TException> context, IBehavior<CreateLoanSagaState, CreateContractRequestedEvent> next)
        where TException : Exception
    {
        // Можно логировать
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("create-loan-contract-activity");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        // не используем
    }

}

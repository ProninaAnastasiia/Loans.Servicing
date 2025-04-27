using Loans.Servicing.Kafka.Events;
using MassTransit;

namespace Loans.Servicing.StateMachines;

public class LoanContractStateMachine : MassTransitStateMachine<LoanContractState> 
{
    public LoanContractStateMachine()
    {
        InstanceState(x => x.CurrentState, Created);

        Event(() => CreateContractRequestedEvent, x => x.CorrelateById(context => context.Message.ApplicationId));

        Initially(
            When(CreateContractRequestedEvent)
               // .Then(context => context.Saga.Value = context.Message.Value)
                .TransitionTo(Created)
        );

        SetCompletedWhenFinalized();
    }

    public State Created { get; private set; }

    public Event<CreateContractRequestedEvent> CreateContractRequestedEvent { get; private set; } 
}
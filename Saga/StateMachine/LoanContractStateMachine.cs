using Loans.Servicing.Kafka.Events;
using Loans.Servicing.Saga.Activities;
using Loans.Servicing.Saga.State;
using MassTransit;
using SagaState = MassTransit.State;

namespace Loans.Servicing.Saga.StateMachine;

public class LoanContractStateMachine : MassTransitStateMachine<CreateLoanSagaState>
{
        public SagaState WaitingForDraft  { get; private set; }
        public SagaState Completed  { get; private set; }

        public Event<CreateContractRequestedEvent> CreateContractRequested { get; private set; }
        public Event<DraftContractCreatedEvent> DraftContractCreated { get; private set; }
        

        public LoanContractStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => CreateContractRequested, x =>
            {
                x.CorrelateById(context => context.Message.ApplicationId);
                x.InsertOnInitial = true;
                x.SetSagaFactory(context => new CreateLoanSagaState
                {
                    CorrelationId = context.Message.ApplicationId,
                    ApplicationId = context.Message.ApplicationId,
                    CreatedAt = DateTime.UtcNow
                });
            });
            Event(() => DraftContractCreated, x => x.CorrelateById(c => c.Message.ApplicationId));

            Initially(
                When(CreateContractRequested)
                    .TransitionTo(WaitingForDraft)
                    .Activity(x => x.OfType<CreateLoanContractActivity>())
            );


            During(WaitingForDraft,
                When(DraftContractCreated)
                    .Then(context =>
                    {
                        context.Instance.ContractId = context.Data.ContractId;
                    })
                    .Send(new Uri("queue:confirm-draft-contract"), context => new
                    {
                        context.Data.ContractId
                    })
                    .TransitionTo(Completed)
                    .Finalize()
            );
            

            SetCompletedWhenFinalized();
        }
}
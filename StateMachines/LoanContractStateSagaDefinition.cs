using MassTransit;

namespace Loans.Servicing.StateMachines;

public class LoanContractStateSagaDefinition : SagaDefinition<LoanContractState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<LoanContractState> sagaConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));

        endpointConfigurator.UseInMemoryOutbox(context);
    }
}

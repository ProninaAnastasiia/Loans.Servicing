using Loans.Servicing.Kafka.Events;

namespace Loans.Servicing.Kafka.Handlers;

public class DraftContractCreatedHandler: IEventHandler<DraftContractCreatedEvent>
{
    public async Task HandleAsync(DraftContractCreatedEvent contractEvent, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("Публикация события для расчета задолженности и графиков платежей");
            // Публикация события для расчета задолженности и графиков платежей
        }
        catch (Exception e)
        {
        }
    }
}
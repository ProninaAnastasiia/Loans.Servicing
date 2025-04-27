using MassTransit;

namespace Loans.Servicing.StateMachines;

class KafkaMessageConsumer :
    IConsumer<KafkaMessage>
{
    public Task Consume(ConsumeContext<KafkaMessage> context)
    {
        return Task.CompletedTask;
    }
}

public record KafkaMessage
{
    public string Text { get; init; }
}
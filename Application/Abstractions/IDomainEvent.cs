namespace BimAiAssistant.Application.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

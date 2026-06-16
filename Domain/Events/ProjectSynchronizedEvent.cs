using BimAiAssistant.Application.Abstractions;

namespace BimAiAssistant.Domain.Events;

public sealed class ProjectSynchronizedEvent : IDomainEvent
{
    public string Project { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

using BimAiAssistant.Application.Abstractions;
using BimAiAssistant.Models;

namespace BimAiAssistant.Application.Commands;

public sealed class SyncProjectCommand : ICommand<string>
{
    public SyncPayload Payload { get; init; } = new();
}

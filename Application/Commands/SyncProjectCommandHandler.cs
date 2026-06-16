using BimAiAssistant.Application.Abstractions;
using BimAiAssistant.Services;

namespace BimAiAssistant.Application.Commands;

public sealed class SyncProjectCommandHandler : ICommandHandler<SyncProjectCommand, string>
{
    private readonly SyncService _syncService;

    public SyncProjectCommandHandler(SyncService syncService)
    {
        _syncService = syncService;
    }

    public Task<string> HandleAsync(SyncProjectCommand command, CancellationToken cancellationToken = default) =>
        _syncService.SyncAsync(command.Payload, cancellationToken: cancellationToken);
}

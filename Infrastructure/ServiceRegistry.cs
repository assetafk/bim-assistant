using BimAiAssistant.Services;

namespace BimAiAssistant.Infrastructure;

public sealed class ServiceRegistry
{
    public SettingsService SettingsService { get; } = new();
    public AuthService AuthService { get; }
    public BackendApiService BackendApiService { get; }

    public ServiceRegistry()
    {
        AuthService = new AuthService(SettingsService);
        BackendApiService = new BackendApiService(SettingsService, AuthService);
    }
}

using BimAiAssistant.Services;

namespace BimAiAssistant.Infrastructure;

public sealed class ServiceRegistry
{
    public SettingsService SettingsService { get; } = new();
    public AuthService AuthService { get; }
    public BackendApiService BackendApiService { get; }
    public RedisCacheService RedisCacheService { get; }

    public ServiceRegistry()
    {
        AuthService = new AuthService(SettingsService);
        RedisCacheService = new RedisCacheService(SettingsService);
        BackendApiService = new BackendApiService(SettingsService, AuthService);
    }
}

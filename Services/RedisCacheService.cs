using BimAiAssistant.Application.Abstractions;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BimAiAssistant.Services;

public sealed class RedisCacheService : ICacheService, IDisposable
{
    private readonly Lazy<ConnectionMultiplexer> _connection;

    public RedisCacheService(SettingsService settingsService)
    {
        _connection = new Lazy<ConnectionMultiplexer>(() =>
        {
            AppSettings settings = settingsService.Load();
            return ConnectionMultiplexer.Connect(settings.RedisConnectionString);
        });
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        RedisValue value = await Database.StringGetAsync(key);
        if (!value.HasValue)
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(value!);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        string json = JsonConvert.SerializeObject(value);
        return Database.StringSetAsync(key, json, ttl);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        Database.KeyDeleteAsync(key);

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }
    }

    private IDatabase Database => _connection.Value.GetDatabase();
}

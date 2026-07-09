using BimAiAssistant.Application.Abstractions;
using BimAiAssistant.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BimAiAssistant.Services;

public sealed class RedisCacheService : ICacheService, IDisposable
{
    private readonly SettingsService _settingsService;
    private ConnectionMultiplexer? _connection;
    private bool _disabled;

    public RedisCacheService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        IDatabase? database = TryGetDatabase();
        if (database is null)
        {
            return default;
        }

        try
        {
            RedisValue value = await database.StringGetAsync(key);
            return value.HasValue ? JsonConvert.DeserializeObject<T>(value!) : default;
        }
        catch (RedisException)
        {
            return default;
        }
        catch (JsonException)
        {
            await RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        IDatabase? database = TryGetDatabase();
        if (database is null)
        {
            return Task.CompletedTask;
        }

        string json = JsonConvert.SerializeObject(value);
        return database.StringSetAsync(key, json, ttl);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        IDatabase? database = TryGetDatabase();
        return database?.KeyDeleteAsync(key) ?? Task.CompletedTask;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private IDatabase? TryGetDatabase()
    {
        if (_disabled)
        {
            return null;
        }

        try
        {
            if (_connection is null || !_connection.IsConnected)
            {
                AppSettings settings = _settingsService.Load();
                if (string.IsNullOrWhiteSpace(settings.RedisConnectionString))
                {
                    _disabled = true;
                    return null;
                }

                var options = ConfigurationOptions.Parse(settings.RedisConnectionString);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 1000;
                _connection = ConnectionMultiplexer.Connect(options);
            }

            return _connection.GetDatabase();
        }
        catch (RedisException)
        {
            _disabled = true;
            return null;
        }
    }
}

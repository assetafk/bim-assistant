using BimAiAssistant.Application.Abstractions;
using BimAiAssistant.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BimAiAssistant.Services;

public sealed class RedisCacheService : ICacheService, IDisposable
{
    private readonly SettingsService _settingsService;
    private ConnectionMultiplexer? _connection;
    private DateTimeOffset _retryAfter = DateTimeOffset.MinValue;

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
            MarkUnavailable();
            return default;
        }
        catch (JsonException)
        {
            await RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        IDatabase? database = TryGetDatabase();
        if (database is null)
        {
            return;
        }

        try
        {
            string json = JsonConvert.SerializeObject(value);
            await database.StringSetAsync(key, json, ttl);
        }
        catch (RedisException)
        {
            MarkUnavailable();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        IDatabase? database = TryGetDatabase();
        if (database is null)
        {
            return;
        }

        try
        {
            await database.KeyDeleteAsync(key);
        }
        catch (RedisException)
        {
            MarkUnavailable();
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private IDatabase? TryGetDatabase()
    {
        if (DateTimeOffset.UtcNow < _retryAfter)
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
                    MarkUnavailable();
                    return null;
                }

                var options = ConfigurationOptions.Parse(settings.RedisConnectionString);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 1000;
                _connection = ConnectionMultiplexer.Connect(options);
            }

            return _connection.GetDatabase();
        }
        catch (Exception ex) when (ex is RedisException or ArgumentException)
        {
            MarkUnavailable();
            return null;
        }
    }

    private void MarkUnavailable()
    {
        _retryAfter = DateTimeOffset.UtcNow.AddMinutes(1);
    }
}

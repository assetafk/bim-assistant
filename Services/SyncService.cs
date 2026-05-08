using System.Net.Http;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Services;

public sealed class SyncService
{
    private readonly SettingsService _settingsService;
    private readonly AuthService _authService;

    public SyncService(SettingsService settingsService, AuthService authService)
    {
        _settingsService = settingsService;
        _authService = authService;
    }

    public async Task<string> SyncAsync(SyncPayload payload, AuthSession? session = null, CancellationToken cancellationToken = default)
    {
        AppSettings settings = _settingsService.Load();
        using var httpClient = new HttpClient { BaseAddress = new Uri(settings.BackendUrl), Timeout = TimeSpan.FromSeconds(60) };
        _authService.ApplyBearerToken(httpClient, session);

        string json = JsonConvert.SerializeObject(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync("/sync/revit-model", content, cancellationToken);

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return body;
    }
}

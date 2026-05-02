using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Services;

public sealed class AuthService
{
    private readonly SettingsService _settingsService;

    public AuthService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<AuthSession> RefreshAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        AppSettings settings = _settingsService.Load();
        using var httpClient = new HttpClient { BaseAddress = new Uri(settings.BackendUrl), Timeout = TimeSpan.FromSeconds(30) };

        var payload = new { refreshToken = session.RefreshToken };
        using var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync("/auth/refresh", content, cancellationToken);

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        return JsonConvert.DeserializeObject<AuthSession>(body) ?? throw new InvalidOperationException("Invalid auth response.");
    }

    public bool CanExecute(UserRole role, string permission) =>
        role switch
        {
            UserRole.Admin => true,
            UserRole.Engineer => permission is "model:read" or "model:export" or "ai:chat" or "report:create",
            UserRole.Viewer => permission is "model:read" or "ai:chat",
            _ => false
        };

    public void ApplyBearerToken(HttpClient httpClient, AuthSession? session)
    {
        if (!string.IsNullOrWhiteSpace(session?.AccessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }
    }
}

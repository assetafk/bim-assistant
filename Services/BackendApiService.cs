using System.Net.Http;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Services;

public sealed class BackendApiService
{
    private readonly SettingsService _settingsService;
    private readonly AuthService _authService;

    public BackendApiService(SettingsService settingsService, AuthService authService)
    {
        _settingsService = settingsService;
        _authService = authService;
    }

    public async Task<IReadOnlyList<WorkProject>> GetProjectsAsync(AuthSession? session = null, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = CreateClient(session);
        string json = await httpClient.GetStringAsync("/projects", cancellationToken);
        return JsonConvert.DeserializeObject<List<WorkProject>>(json) ?? [];
    }

    public async Task<BuildingModel> GetModelAsync(string project, AuthSession? session = null, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = CreateClient(session);
        string json = await httpClient.GetStringAsync($"/model?project={Uri.EscapeDataString(project)}", cancellationToken);
        return JsonConvert.DeserializeObject<BuildingModel>(json) ?? new BuildingModel();
    }

    public Task<string> ValidateAsync(BuildingModel model, AuthSession? session = null, CancellationToken cancellationToken = default) =>
        PostAsync("/validation", model, session, cancellationToken);

    public Task<string> ChatAsync(BackendChatRequest request, AuthSession? session = null, CancellationToken cancellationToken = default) =>
        PostAsync("/chat", request, session, cancellationToken);

    public Task<string> ReportAsync(SyncPayload payload, AuthSession? session = null, CancellationToken cancellationToken = default) =>
        PostAsync("/report", payload, session, cancellationToken);

    public Task<string> ExportAsync(BackendExportRequest request, AuthSession? session = null, CancellationToken cancellationToken = default) =>
        PostAsync("/export", request, session, cancellationToken);

    private async Task<string> PostAsync(string route, object payload, AuthSession? session, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = CreateClient(session);
        string json = JsonConvert.SerializeObject(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync(route, content, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return body;
    }

    private HttpClient CreateClient(AuthSession? session)
    {
        AppSettings settings = _settingsService.Load();
        var httpClient = new HttpClient { BaseAddress = new Uri(settings.BackendUrl), Timeout = TimeSpan.FromSeconds(60) };
        _authService.ApplyBearerToken(httpClient, session);
        return httpClient;
    }
}

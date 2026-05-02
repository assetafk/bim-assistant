using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BimAiAssistant.Services;

public sealed class OpenAIService
{
    private readonly SettingsService _settingsService;

    public OpenAIService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<string> AskAsync(string userMessage, BuildingModel model, CancellationToken cancellationToken = default)
    {
        AppSettings settings = _settingsService.Load();
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        }

        object payload = CreatePayload(settings, userMessage, model);
        string json = JsonConvert.SerializeObject(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync(settings.ApiUrl, content, cancellationToken);

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        return ExtractAnswer(body);
    }

    private static object CreatePayload(AppSettings settings, string userMessage, BuildingModel model)
    {
        object context = new
        {
            walls = model.Statistics.Walls,
            doors = model.Statistics.Doors,
            windows = model.Statistics.Windows,
            floors = model.Statistics.Floors,
            rooms = model.Statistics.Rooms,
            buildingArea = model.Statistics.BuildingArea,
            organization = model.Organization,
            project = model.Project
        };

        if (settings.ApiUrl.Contains("/v1/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return new
            {
                model = settings.ModelName,
                temperature = settings.Temperature,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You answer questions about a Revit BIM model using only the supplied JSON context."
                    },
                    new
                    {
                        role = "user",
                        content = JsonConvert.SerializeObject(new { question = userMessage, model = context })
                    }
                }
            };
        }

        return new
        {
            message = userMessage,
            model = settings.ModelName,
            temperature = settings.Temperature,
            context
        };
    }

    private static string ExtractAnswer(string body)
    {
        JObject token = JObject.Parse(body);

        return token.SelectToken("choices[0].message.content")?.Value<string>()
            ?? token.SelectToken("message.content")?.Value<string>()
            ?? token.SelectToken("answer")?.Value<string>()
            ?? token.SelectToken("response")?.Value<string>()
            ?? body;
    }
}

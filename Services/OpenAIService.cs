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
            counts = new
            {
                walls = model.Statistics.Walls,
                doors = model.Statistics.Doors,
                windows = model.Statistics.Windows,
                columns = model.Columns.Count,
                rooms = model.Statistics.Rooms,
                levels = model.Levels.Count,
                families = model.Families.Count,
                views = model.Views.Count,
                sheets = model.Sheets.Count,
                dimensions = model.Dimensions.Count,
                floors = model.Statistics.Floors,
                buildingArea = model.Statistics.BuildingArea
            },
            organization = model.Organization,
            project = model.Project,
            structure = new
            {
                walls = model.Walls.Take(200),
                doors = model.Doors.Take(200),
                windows = model.Windows.Take(200),
                columns = model.Columns.Take(200),
                rooms = model.Rooms.Take(200),
                levels = model.Levels,
                families = model.Families.Take(200),
                views = model.Views.Take(200),
                sheets = model.Sheets.Take(200),
                dimensions = model.Dimensions.Take(200)
            },
            queryIndexes = new
            {
                wallsWithoutMaterial = model.Walls.Where(wall => string.IsNullOrWhiteSpace(wall.Material)),
                roomsWithoutArea = model.Rooms.Where(room => room.Area <= 0),
                doorsWithoutMark = model.Doors.Where(door => string.IsNullOrWhiteSpace(door.Mark)),
                windowsWithoutLevel = model.Windows.Where(window => string.IsNullOrWhiteSpace(window.Level)),
                windowsByLevel = model.Windows
                    .GroupBy(window => string.IsNullOrWhiteSpace(window.Level) ? "Unknown" : window.Level)
                    .ToDictionary(group => group.Key, group => group.Count())
            },
            availableActions = new[]
            {
                new
                {
                    name = "RenameDoorsByCompanyStandard",
                    description = "Rename all doors using the Maybeworks mark format MW-DR-{LEVEL}-{NUMBER}."
                },
                new
                {
                    name = "FillMissingParameters",
                    description = "Fill or flag missing required parameters such as door marks and wall materials."
                },
                new
                {
                    name = "CreateDoorSchedule",
                    description = "Create a Revit door schedule for the active project."
                }
            }
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
                        content = "You are a Maybeworks BIM assistant. Understand natural-language Revit model queries and answer using only the supplied JSON context. Return exact element ids when the user asks to find elements. If the user asks to modify the Revit model, do not invent operations. Return a short explanation and an action JSON object with one of the allowed availableActions. The desktop plugin will ask for confirmation and execute the action through a safe Revit transaction."
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
            context,
            actionSchema = new
            {
                type = "object",
                required = new[] { "action" },
                example = new
                {
                    action = "RenameDoorsByCompanyStandard",
                    arguments = new { }
                }
            }
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

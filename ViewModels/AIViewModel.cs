using System.Collections.ObjectModel;
using System.Windows.Input;
using BimAiAssistant.Models;
using BimAiAssistant.Services;
using BimAiAssistant.Utils;

namespace BimAiAssistant.ViewModels;

public sealed class AIViewModel : ObservableObject
{
    private readonly RevitService _revitService;
    private readonly OpenAIService _openAIService;
    private readonly SettingsService _settingsService;
    private string _message = string.Empty;
    private bool _isBusy;

    public AIViewModel(RevitService revitService, OpenAIService openAIService, SettingsService settingsService)
    {
        _revitService = revitService;
        _openAIService = openAIService;
        _settingsService = settingsService;
        SendCommand = new AsyncRelayCommand(SendAsync, () => !string.IsNullOrWhiteSpace(Message));
        Settings = _settingsService.Load();
        Messages.Add(new ChatMessage
        {
            Author = "Assistant",
            Text = "Ask me about walls, doors, windows, rooms or total building area."
        });
    }

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    public AppSettings Settings { get; }

    public string Message
    {
        get => _message;
        set
        {
            if (SetProperty(ref _message, value) && SendCommand is AsyncRelayCommand command)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand SendCommand { get; }

    private async Task SendAsync()
    {
        string userMessage = Message.Trim();
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return;
        }

        Message = string.Empty;
        Messages.Add(new ChatMessage { Author = "You", Text = userMessage });
        IsBusy = true;

        try
        {
            BuildingModel model = _revitService.GetBuildingModel();
            string answer = await _openAIService.AskAsync(userMessage, model);
            Messages.Add(new ChatMessage { Author = "Assistant", Text = answer });
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatMessage { Author = "Error", Text = ex.Message });
        }
        finally
        {
            IsBusy = false;
        }
    }
}

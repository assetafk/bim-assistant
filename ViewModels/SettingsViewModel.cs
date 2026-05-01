using System.Windows;
using System.Windows.Input;
using BimAiAssistant.Models;
using BimAiAssistant.Services;
using BimAiAssistant.Utils;

namespace BimAiAssistant.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private string _apiUrl;
    private string _apiKey;
    private string _modelName;
    private double _temperature;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        AppSettings settings = _settingsService.Load();
        _apiUrl = settings.ApiUrl;
        _apiKey = settings.ApiKey;
        _modelName = settings.ModelName;
        _temperature = settings.Temperature;
        SaveCommand = new RelayCommand(Save);
    }

    public string ApiUrl
    {
        get => _apiUrl;
        set => SetProperty(ref _apiUrl, value);
    }

    public string ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public string ModelName
    {
        get => _modelName;
        set => SetProperty(ref _modelName, value);
    }

    public double Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    public ICommand SaveCommand { get; }

    private void Save(object? parameter)
    {
        _settingsService.Save(new AppSettings
        {
            ApiUrl = ApiUrl,
            ApiKey = ApiKey,
            ModelName = ModelName,
            Temperature = Temperature
        });

        if (parameter is Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}

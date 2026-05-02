# BIM AI Assistant for Autodesk Revit

WPF/MVVM Revit add-in on .NET 8 that adds an `AI Tools` ribbon tab and provides model statistics, AI chat, JSON export, model validation, settings and PDF reporting.

## Features

- `Model Statistics`: counts walls, doors, windows, floors and rooms through Revit API.
- `AI Assistant`: sends the current model context as JSON to a configured REST endpoint.
- `Export Model`: writes `building.json` to `Documents\BimAiAssistant`.
- `Find Errors`: checks rooms without area, doors without mark, walls without material and windows without a valid level.
- `Settings`: stores API URL, API key, model name and temperature in `%APPDATA%\BimAiAssistant\settings.json`.
- `Generate Report`: writes `building-report.pdf` to `Documents\BimAiAssistant`.

## Build

The project expects Revit 2025 by default:

```powershell
dotnet build
```

For another Revit installation path:

```powershell
dotnet build -p:RevitApiPath="C:\Program Files\Autodesk\Revit 2026"
```

When Revit API DLLs are not available, compile-time stubs are enabled only to validate the C#/WPF source. On a Revit machine, the real `RevitAPI.dll` and `RevitAPIUI.dll` are used.

## Install

1. Build the project.
2. Copy `bin\Debug\net8.0-windows\BimAiAssistant.dll` and dependencies to:

   ```text
   %APPDATA%\Autodesk\Revit\Addins\2025\BimAiAssistant
   ```

3. Copy `BimAiAssistant.addin` to:

   ```text
   %APPDATA%\Autodesk\Revit\Addins\2025
   ```

4. Start Revit and open the `AI Tools` tab.

## API Contract

Generic `/chat` endpoints receive:

```json
{
  "message": "How many doors are in the model?",
  "model": "llama3.1",
  "temperature": 0.2,
  "context": {
    "walls": 245,
    "doors": 67,
    "windows": 134,
    "floors": 12,
    "rooms": 98,
    "buildingArea": 1250.5,
    "project": "Office"
  }
}
```

OpenAI-compatible `/v1/chat/completions` URLs are detected automatically and use the standard `messages` payload with bearer authorization.

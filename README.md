# Maybeworks BIM AI Assistant for Autodesk Revit

Production-ready commercial Revit plugin prototype for Maybeworks. The solution automates BIM model analysis, AI-assisted model queries, validation, JSON export and PDF reporting for architects, BIM managers, engineers and project managers.

The desktop add-in is built with C# / .NET 8 / WPF / MVVM and targets Revit 2023+, Revit 2024+ and Revit 2025+ through configurable Revit API references.

## Features

- `Model Statistics`: counts walls, doors, windows, floors and rooms through Revit API.
- `AI Assistant`: sends the current Maybeworks model context as JSON to a configured REST endpoint.
- `Export Model`: writes `building.json` to `Documents\BimAiAssistant`.
- `Find Errors`: checks rooms without area, doors without mark, walls without material and windows without a valid level.
- `Settings`: stores organization, backend URL, API URL, API key, model name and temperature in `%APPDATA%\BimAiAssistant\settings.json`.
- `Generate Report`: writes `building-report.pdf` to `Documents\BimAiAssistant`.
- `RBAC foundation`: includes Admin, Engineer and Viewer roles for secured production workflows.

## Commercial MVP Scope

Maybeworks BIM AI Assistant is structured as a production plugin, not a one-file sample:

- Desktop: C#, .NET 8, WPF, MVVM.
- Autodesk: Revit API, Ribbon API, External Commands, External Events-ready architecture.
- Backend target: FastAPI.
- AI providers: OpenAI, Ollama, Azure OpenAI.
- Data platform target: PostgreSQL, Redis, RabbitMQ, MinIO.
- Observability target: Prometheus, Grafana, Serilog.
- Security target: SSO, JWT, refresh tokens and RBAC.

See [docs/MVP.md](docs/MVP.md) for the detailed Maybeworks MVP specification.

## Build

The project expects Revit 2025 by default:

```powershell
dotnet build
```

For another Revit installation path:

```powershell
dotnet build -p:RevitApiPath="C:\Program Files\Autodesk\Revit 2026"
```

For Revit 2023 or 2024:

```powershell
dotnet build -p:RevitApiPath="C:\Program Files\Autodesk\Revit 2023"
dotnet build -p:RevitApiPath="C:\Program Files\Autodesk\Revit 2024"
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
    "organization": "Maybeworks",
    "project": "Office"
  }
}
```

OpenAI-compatible `/v1/chat/completions` URLs are detected automatically and use the standard `messages` payload with bearer authorization.

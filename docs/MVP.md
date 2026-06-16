# Maybeworks BIM AI Assistant MVP

## Goal

Develop a production-ready Revit plugin for Maybeworks that automates BIM workflows and connects Autodesk Revit models with AI, backend services and enterprise data infrastructure.

## Supported Autodesk Versions

- Revit 2023+
- Revit 2024+
- Revit 2025+

## Technology Stack

Desktop:

- C#
- .NET 8
- WPF
- MVVM
- async/await

Autodesk:

- Revit API
- Ribbon API
- External Commands
- External Events

Backend:

- FastAPI
- Swagger/OpenAPI

AI:

- OpenAI
- Ollama
- Azure OpenAI

Infrastructure:

- PostgreSQL
- Redis
- RabbitMQ
- MinIO
- Prometheus
- Grafana
- Serilog
- Docker
- CI/CD

## Users

- Architect
- BIM Manager
- Engineer
- Project Manager

## Work Projects

Each authorized user can open one of the Maybeworks work projects:

- Business Center
- Shopping Mall
- School
- Hospital

The desktop plugin keeps the selected project context and sends it to the backend together with Revit model metadata.

## Authorization Requirements

The MVP is designed for enterprise authorization:

- SSO
- JWT access token
- Refresh token
- RBAC

Roles:

- Admin
- Engineer
- Viewer

Initial permission model:

| Permission | Admin | Engineer | Viewer |
| --- | --- | --- | --- |
| model:read | Yes | Yes | Yes |
| model:export | Yes | Yes | No |
| ai:chat | Yes | Yes | Yes |
| report:create | Yes | Yes | No |
| settings:manage | Yes | No | No |

## MVP Modules

- Ribbon tab `AI Tools`.
- Model statistics from Revit API.
- AI assistant with model-context JSON payload.
- JSON export for integration with backend services.
- Validation Engine with built-in BIM QA checks.
- Rule Engine with JSON rules that can be changed without recompilation.
- Local settings storage.
- AI PDF report generation.
- Dashboard with BIM KPI charts.
- Project synchronization with backend and database.
- Change history and undo for tracked operations.
- Multi-format export.
- Auth/RBAC service foundation for secured production workflows.

## Revit Model Structure

The plugin extracts the following structure through Revit API:

- Walls
- Doors
- Windows
- Columns
- Rooms
- Levels
- Families
- Views
- Sheets
- Dimensions

This structure is normalized into DTO models before export or AI processing, so the backend and LLM providers do not depend on Revit API types directly.

## AI Chat Requirements

The AI chat must understand natural-language BIM queries and answer only from the current model context.

Supported MVP query examples:

- Show all walls without material.
- Find rooms without area.
- How many windows are on the second level?

The plugin sends both summary counts and query-oriented indexes:

- `wallsWithoutMaterial`
- `roomsWithoutArea`
- `doorsWithoutMark`
- `windowsWithoutLevel`
- `windowsByLevel`

When the user asks to find elements, the assistant should return element ids and short explanations.

## AI Actions

The assistant can request safe Revit tools instead of only answering with text. The LLM never executes arbitrary code. It returns an action name and arguments, then the desktop plugin validates the action, asks the user for confirmation and runs the operation through a Revit transaction.

Supported MVP actions:

- `RenameDoorsByCompanyStandard`: rename all doors using the Maybeworks standard `MW-DR-{LEVEL}-{NUMBER}`.
- `FillMissingParameters`: fill or flag missing required parameters, including missing door marks and walls without material.
- `CreateDoorSchedule`: create a Revit door schedule in the active project.

Example user requests:

- Rename all doors by company standard.
- Fill missing parameters.
- Create a door schedule.

Action response contract:

```json
{
  "message": "I can rename all doors using the Maybeworks standard. Please confirm before applying changes.",
  "action": "RenameDoorsByCompanyStandard",
  "arguments": {}
}
```

Execution rules:

- Only whitelisted actions are allowed.
- Model-changing actions require user confirmation.
- Actions run inside `Autodesk.Revit.DB.Transaction`.
- Failed actions return a structured result with message and affected element count.
- Future production execution should be routed through External Events when called from modeless WPF windows.

## Validation Engine

The Validation Engine runs built-in model quality checks:

- Empty parameters.
- Invalid family naming.
- Missing material.
- Element intersections.
- Duplicate marks or identifiers.
- Unused types.
- Invalid or missing levels.

Each validation result contains:

- Rule id.
- Severity.
- Element type.
- Element id.
- Message.

## Rule Engine

Maybeworks can create and edit company-specific validation rules without recompiling the Revit plugin. Rules are stored as JSON in:

```text
%APPDATA%\BimAiAssistant\rules.json
```

Default examples:

```json
[
  {
    "id": "MW-WALL-FIRE-RATING",
    "name": "All walls must have FireRating",
    "category": "Walls",
    "check": "RequiredParameter",
    "parameterName": "FireRating",
    "severity": "Error",
    "enabled": true
  },
  {
    "id": "MW-DOOR-WIDTH",
    "name": "Doors must have Width",
    "category": "Doors",
    "check": "RequiredParameter",
    "parameterName": "Width",
    "severity": "Error",
    "enabled": true
  }
]
```

Supported MVP rule check:

- `RequiredParameter`: validates that a parameter exists and is not empty.

Supported MVP categories:

- Walls
- Doors
- Windows
- Columns
- Rooms

## AI Report

The plugin generates a PDF AI report for the active Maybeworks project.

Report contents:

- Validation errors and warnings.
- Model statistics.
- Element counts.
- AI recommendations.
- Problem zones.

The report combines deterministic Revit API data with AI-style recommendations derived from validation results. For production deployment, recommendation generation can be moved to OpenAI, Azure OpenAI or Ollama through the same backend contract used by AI Chat.

Output:

```text
Documents\BimAiAssistant\ai-building-report.pdf
```

## Dashboard

The Dashboard provides visual KPI charts for the active Revit model.

Charts:

- Error count.
- Room count.
- Level count.
- Door count.
- Window count.
- Material count.

The dashboard is available from the `AI Tools` ribbon tab and uses the same Revit API model data and Validation Engine results as AI Report.

## Export

The plugin supports multi-format export:

- JSON
- Excel
- PDF
- CSV

Export output folder:

```text
Documents\BimAiAssistant
```

## Sync

The plugin supports project synchronization through the production data pipeline:

```text
Revit -> Plugin -> Backend -> Database
```

The sync payload contains:

- Organization.
- Project name.
- Sync timestamp.
- Normalized Revit model structure.
- Validation issues.
- Change history.

Desktop command:

- `Sync`

Backend target:

- `POST /sync/revit-model`

The backend is responsible for persisting synchronized project snapshots into PostgreSQL and storing large exported artifacts in MinIO when needed.

## Change History

Every tracked model-changing operation stores an audit trail:

- Who changed it.
- When it changed.
- What action was executed.
- What element changed.
- Parameter name.
- Old value.
- New value.

Local audit storage:

```text
%APPDATA%\BimAiAssistant\change-history.json
```

The history is included in synchronization payloads so the backend/database can keep a full project audit log.

## Undo

Any tracked parameter change can be undone through the `Undo` ribbon command.

Undo behavior:

- Loads the latest non-undone operation from local change history.
- Opens a Revit transaction.
- Restores old parameter values in reverse order.
- Marks the operation as undone.

For production use, destructive object-creation operations should be represented as explicit reversible commands with element deletion/restoration metadata.

## Backend API Direction

Planned FastAPI endpoints:

- `GET /projects`
- `GET /model`
- `POST /validation`
- `POST /chat`
- `POST /report`
- `POST /export`
- `POST /auth/sso/callback`
- `POST /auth/refresh`
- `POST /models/import`
- `POST /sync/revit-model`
- `GET /projects/{id}/changes`
- `POST /projects/{id}/changes/{operationId}/undo`
- `GET /models/{id}/statistics`
- `POST /reports`

The Revit plugin keeps the desktop workflow responsive and delegates enterprise concerns such as long-running processing, queueing, storage and monitoring to backend services.

## Architecture Requirements

The codebase includes production architecture boundaries and abstractions:

- Dependency Injection / composition root.
- Repository Pattern.
- Unit Of Work.
- CQRS.
- MediatR-ready command/query handlers.
- FluentValidation-ready validator abstraction.
- AutoMapper-ready mapper abstraction.
- Clean Architecture.
- SOLID.
- Domain Events.
- JWT.
- Refresh Token.
- Swagger/OpenAPI.
- Docker.
- CI/CD.

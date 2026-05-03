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

Autodesk:

- Revit API
- Ribbon API
- External Commands
- External Events

Backend:

- FastAPI

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
- Model validation rules.
- Local settings storage.
- PDF report generation.
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

## Backend API Direction

Planned FastAPI endpoints:

- `POST /auth/sso/callback`
- `POST /auth/refresh`
- `POST /chat`
- `POST /models/import`
- `GET /models/{id}/statistics`
- `POST /reports`

The Revit plugin keeps the desktop workflow responsive and delegates enterprise concerns such as long-running processing, queueing, storage and monitoring to backend services.

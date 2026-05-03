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

## Backend API Direction

Planned FastAPI endpoints:

- `POST /auth/sso/callback`
- `POST /auth/refresh`
- `POST /chat`
- `POST /models/import`
- `GET /models/{id}/statistics`
- `POST /reports`

The Revit plugin keeps the desktop workflow responsive and delegates enterprise concerns such as long-running processing, queueing, storage and monitoring to backend services.

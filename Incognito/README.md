# Incognito PAS

Incognito PAS is a Project Approval System with incognito matching between students and supervisors.

## Current Architecture

This repository now uses an API-first backend plus Angular SPA frontend:

- Backend: ASP.NET Core Web API, EF Core, ASP.NET Identity, role-based authorization, audit logging
- Frontend: Angular standalone SPA with role-based routing, guards, interceptor, typed API services, Angular Material UI

Razor MVC views are removed from the active runtime flow.

## Blind Matching Rules

The server enforces blind workflow rules:

1. Students submit proposals anonymously.
2. Supervisors browse only blind proposal DTOs (no student identity fields).
3. Supervisors express interest.
4. Identity is revealed only when a valid supervisor confirmation completes.
5. Module Leader can reassign confirmed matches using controlled workflow.

## Role Journeys (Angular SPA)

- Student:
	- Dashboard: `/student`
	- Proposals list: `/student/proposals`
	- Create proposal: `/student/create`
	- Proposal details: `/student/proposals/:id`
	- Proposal edit: `/student/proposals/:id/edit`
- Supervisor:
	- Dashboard: `/supervisor`
	- Browse blind proposals: `/supervisor/browse`
	- Interests queue: `/supervisor/interests`
	- Confirmed matches: `/supervisor/confirmed`
	- Expertise profile: `/supervisor/expertise`
- Module Leader:
	- Dashboard: `/module-leader`
	- Match oversight/reassignment: `/module-leader/matches`
	- Research area CRUD: `/module-leader/research-areas`
	- User directory: `/module-leader/users`
- SysAdmin:
	- Dashboard: `/admin`
	- User management: `/admin/users`
	- Migration status: `/admin/migrations`
	- Audit logs: `/admin/audit-logs`

## Prerequisites

- .NET SDK 10.x
- SQL Server (LocalDB or SQL Server instance)
- Node.js 22.x
- npm 11.x

## Run Backend API

```bash
dotnet restore
dotnet run
```

API and Swagger (development) run on the backend HTTPS URL configured by launch settings.

## Run Angular Frontend

```bash
cd frontend
npm install
npm run start
```

Frontend runs at `http://localhost:4200`.

Note: Angular scripts use increased Node heap (`--max_old_space_size=4096`) to reduce JS heap OOM failures during large builds/tests.

## Build Commands

```bash
# Backend
dotnet build

# Frontend
cd frontend
npm run build
```

Frontend production output is generated at:

`frontend/dist/frontend`

## Test Commands

```bash
# Backend (currently validates solution build/state)
dotnet test

# Frontend CI-style unit tests
cd frontend
npm run test:ci
```

## Default Seed Credentials

| Role         | Email                          | Password      |
|--------------|--------------------------------|---------------|
| SysAdmin     | admin@incognito.ac.lk         | Admin@123456  |
| ModuleLeader | leader@incognito.ac.lk        | Leader@123456 |
| Supervisor   | supervisor1@incognito.ac.lk   | Super@123456  |
| Supervisor   | supervisor2@incognito.ac.lk   | Super@123456  |
| Student      | student1@incognito.ac.lk      | Student@123   |
| Student      | student2@incognito.ac.lk      | Student@123   |

## API Surface (Role Scoped)

- `/api/auth/*`
- `/api/student/*`
- `/api/supervisor/*`
- `/api/moduleleader/*`
- `/api/admin/*`

## Repository Structure

```text
IncognitoPAS/
├── Controllers/Api/        # API controllers
├── Data/                   # DbContext and seed/bootstrap
├── DTOs/                   # DTO contracts
├── Models/                 # Domain entities
├── Services/               # Business logic layer
├── frontend/               # Angular SPA
└── Migrations/             # EF Core migrations
```
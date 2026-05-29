# Simple Todo API

Repository: [github.com/waggyman/simple-api-dotnet](https://github.com/waggyman/simple-api-dotnet)

A small REST API for managing personal todo lists. You can create an account, sign in, and manage your own todos. Data is stored in a local SQLite database file.

Built with ASP.NET Core 10, JWT authentication, and Entity Framework Core.

## What it does

- **Register / login** — email and password; the API returns a JWT token.
- **Todos** — create, list, update, and delete todos. Each user only sees their own items.
- **Health check** — `GET /health` is public and reports whether the API and database are reachable.

Default URL when running locally: **http://localhost:5081**

## Requirements

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) (includes the `dotnet` CLI).

To build a standalone app that runs **without** installing the SDK, see [Build a standalone app](#build-a-standalone-app) below. End users only need the published binary, not the SDK.

Optional:

- **Git** — to clone the repository
- **Docker** — to run via container (see [Docker](#docker-optional))

## Clone and install

```bash
git clone https://github.com/waggyman/simple-api-dotnet.git
cd simple-api-dotnet
dotnet restore
```

This downloads NuGet packages. You only need to run `dotnet restore` once after cloning (or when dependencies change).

## Local setup

1. **JWT secret** — the API needs a signing key of at least 32 characters.

   Either edit `appsettings.json` (`Jwt:Key`), or use user secrets (recommended for local dev):

   ```bash
   dotnet user-secrets set "Jwt:Key" "your-local-secret-at-least-32-characters-long"
   ```

2. **Database** — SQLite file `todos.db` is created automatically in the project folder when the app starts in Development (migrations run on startup).

3. **Port** — HTTP listens on port **5081** (see `Properties/launchSettings.json`).

## Run locally (with SDK)

From the project root:

```bash
dotnet run --launch-profile http
```

Check that it works:

```bash
curl http://localhost:5081/health
```

Use `simple-api.http` in VS Code / Rider for sample requests, or any HTTP client.

### Auto-restart on file changes

```bash
chmod +x watch.sh
./watch.sh
```

On Linux, if `dotnet watch` fails with an inotify limit error, the script enables polling. Alternatively:

```bash
DOTNET_USE_POLLING_FILE_WATCHER=true dotnet watch run --launch-profile http
```

### Tests

```bash
dotnet test
```

## Project structure

```
simple-api-dotnet/
├── Program.cs                 # Application entry point
├── appsettings.json           # Configuration (connection string, JWT)
├── appsettings.Development.json
├── simple-api.csproj          # Project and package references
├── simple-api.sln             # Solution (app + tests)
├── simple-api.http            # Sample HTTP requests for the IDE
├── watch.sh                   # dotnet watch helper (Linux)
├── Dockerfile                 # Container build
│
├── Api/                       # HTTP layer
│   ├── Controllers/           # Auth and todo endpoints
│   └── Extensions/            # DI, health check, helpers
│
├── Application/               # Business logic
│   ├── Contracts/             # Service interfaces
│   ├── Services/              # Auth and todo use cases
│   ├── Dtos/                  # Request/response models
│   └── Common/                # Result type for service outcomes
│
├── Domain/                    # Core models
│   └── Entities/              # User, Todo
│
├── Infrastructure/            # Technical details
│   ├── Persistence/           # EF Core DbContext and migrations
│   └── Security/              # JWT settings and token creation
│
├── Properties/
│   └── launchSettings.json    # Local URLs and environment
│
└── tests/
    └── simple-api.Tests/      # Integration tests
```

| Folder | Role |
|--------|------|
| `Api` | Controllers and wiring for HTTP |
| `Application` | Rules and orchestration (no HTTP or SQL details) |
| `Domain` | User and Todo entities |
| `Infrastructure` | Database and JWT implementation |
| `tests` | Automated API tests |

## API overview

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health` | No | Health and database check |
| POST | `/api/auth/register` | No | Create account, returns JWT |
| POST | `/api/auth/login` | No | Sign in, returns JWT |
| GET | `/api/todos` | Bearer | List your todos |
| GET | `/api/todos/{id}` | Bearer | Get one todo |
| POST | `/api/todos` | Bearer | Create todo |
| PATCH | `/api/todos/{id}` | Bearer | Update todo |
| DELETE | `/api/todos/{id}` | Bearer | Delete todo |

Protected routes need a header:

```http
Authorization: Bearer <your-token>
```

Example flow:

```bash
# Register
curl -s -X POST http://localhost:5081/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","password":"password123"}'

# Use the "token" value from the response
export TOKEN="<paste-token-here>"

curl -s http://localhost:5081/api/todos -H "Authorization: Bearer $TOKEN"
```

## Configuration

| Setting | Purpose |
|---------|---------|
| `ConnectionStrings:Default` | SQLite path (default `Data Source=todos.db`) |
| `Jwt:Key` | Secret for signing tokens (min. 32 characters) |
| `Jwt:Issuer` / `Jwt:Audience` | Token validation |
| `Jwt:ExpiresInMinutes` | How long tokens stay valid |
| `Database:ApplyMigrationsOnStartup` | Run EF migrations when the app starts |

Environment variables use `__` instead of `:` (example: `Jwt__Key=your-secret`).

In **Development**, migrations run automatically. For a published build, set:

```bash
export ASPNETCORE_ENVIRONMENT=Development
```

or:

```bash
export Database__ApplyMigrationsOnStartup=true
```

## Database files

- `todos.db` — main database
- `todos.db-wal`, `todos.db-shm` — normal while the app is running (SQLite WAL mode)

To reset (stop the app first):

```bash
rm -f todos.db todos.db-wal todos.db-shm
dotnet ef database update
```

(`dotnet ef` requires the EF tools: `dotnet tool install -g dotnet-ef`.)

## Build a standalone app

You can publish a **self-contained** build that bundles the .NET runtime. On the target machine you run the executable directly — no `dotnet` CLI required.

Publish from the project file (not the `.sln`):

### Linux (x64)

On Linux:

```bash
dotnet publish simple-api.csproj -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish/linux-x64
```

Output: `publish/linux-x64/simple-api` (single executable, ~100 MB).

Run it:

```bash
cd publish/linux-x64
export ASPNETCORE_URLS=http://localhost:5081
export ASPNETCORE_ENVIRONMENT=Development
./simple-api
```

Or with migrations in Production mode:

```bash
export ASPNETCORE_URLS=http://localhost:5081
export Database__ApplyMigrationsOnStartup=true
export Jwt__Key="your-secret-at-least-32-characters-long"
./simple-api
```

### Windows (x64, `.exe`)

On Windows, or cross-compile from Linux/macOS with the SDK installed:

```bash
dotnet publish simple-api.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish/win-x64
```

Output: `publish/win-x64/simple-api.exe`

Run on Windows (Command Prompt or PowerShell):

```cmd
cd publish\win-x64
set ASPNETCORE_URLS=http://localhost:5081
set ASPNETCORE_ENVIRONMENT=Development
simple-api.exe
```

### Notes on published builds

- Copy `appsettings.json` is included in the publish folder; override settings with environment variables.
- The SQLite database file is created in the **current working directory** where you start the app.
- `PublishSingleFile=true` produces one main binary; first startup may be slightly slower while extracting.
- ARM Mac/Linux: use `-r osx-arm64`, `linux-arm64`, etc., instead of `x64` if needed.

## Docker (optional)

```bash
docker build -t simple-api .
docker run -p 8080:8080 \
  -e Jwt__Key="production-secret-at-least-32-characters-long" \
  simple-api
```

API: http://localhost:8080  
Health: http://localhost:8080/health

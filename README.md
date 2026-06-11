# etcd-manager

A web-based management UI for [etcd](https://etcd.io/) — the distributed key-value store used for configuration and service discovery. Built with .NET 8 and Angular, deployable as a single Docker container.

## Features

- **Connection management** — add, test, update, and delete multiple etcd endpoints
- **Key-value explorer** — browse, search, create, update, and delete keys; view full revision history
- **Bulk import** — load nodes from JSON/YAML files
- **Snapshots** — backup and restore etcd configuration state
- **Authentication** — JWT-based login with access/refresh token separation and login rate limiting
- **Production-ready** — non-root Docker container, security headers, no stack traces exposed in responses

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8, Entity Framework Core, SQLite |
| Architecture | CQRS with MediatR, FluentValidation, Mapster |
| Auth | JWT Bearer, BCrypt.Net |
| etcd client | dotnet-etcd |
| Frontend | Angular 17+, PrimeNG, RxJS |
| Container | Docker (multi-stage), NGINX, etcd-client CLI |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and [pnpm](https://pnpm.io/)
- A running etcd cluster

### Run in Development

**Backend**

```bash
cd api
dotnet restore
dotnet run
# API available at http://localhost:5000
```

**Frontend**

```bash
cd app/etcd-manager-ui
pnpm install
pnpm start
# UI available at http://localhost:4200
```

### Run with Docker

```bash
docker run -d \
  -p 80:80 \
  -p 81:81 \
  -e Jwt__Key=<your-secret-key> \
  -e ROOT_ACCOUNT_PASSWORD=<root-password> \
  t2nh/etcd-manager:1.0.3
```

The API is served on port **80** and the Angular SPA on port **81**.

## Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `Jwt__Key` | Yes | — | JWT signing secret (must be set or app won't start) |
| `ROOT_ACCOUNT_PASSWORD` | Yes | — | Password for the initial root account |
| `Jwt__Issuer` | No | `etcdmanager.internal` | JWT issuer claim |
| `Jwt__Audience` | No | `etcdmanager.internal` | JWT audience claim |
| `AllowedOrigins` | No | `["http://localhost:4200","http://localhost:80"]` | CORS whitelist |
| `ConnectionStrings__EtcdManager` | No | `{wwwroot}/data/etcd-manager.db` | SQLite connection string |
| `BASE_HREF` | No | `/` | Angular app base href (useful behind a reverse proxy) |

## Project Structure

```
etcd-manager/
├── api/                        # .NET 8 backend
│   ├── Controllers/            # Auth, EtcdConnections, KeyValues endpoints
│   ├── Domain/                 # Entities (AppUser, EtcdConnection, KeyValue, Snapshot …)
│   ├── ApplicationService/     # CQRS Commands & Queries
│   ├── Infrastructure/         # EF Core + SQLite data layer
│   └── Core/                   # Shared utilities, exceptions, attributes
├── app/etcd-manager-ui/        # Angular frontend
│   └── src/
├── Dockerfile                  # Multi-stage production build
├── nginx.conf.template         # NGINX config (SPA + security headers)
├── docker-entrypoint.sh        # Container init (env validation, config substitution)
├── deploy.sh                   # Linux deployment helper
└── deploy-mac.sh               # macOS deployment helper
```

## Building for Production

```bash
# 1. Build backend
cd api
dotnet publish -c Release -o ./bin/Release/Publish

# 2. Build frontend
cd ../app/etcd-manager-ui
pnpm run build-prod

# 3. Build Docker image
cd ../..
docker build -t t2nh/etcd-manager:1.0.3 .
```

## Security

- Passwords hashed with BCrypt
- Login rate-limited to 5 attempts per minute
- Access and refresh tokens are strictly separated
- NGINX serves security headers (`X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, `X-XSS-Protection`)
- Container runs as a non-root user
- Source maps are disabled in production builds
- No secrets committed — use environment variables or a secrets manager

## License

MIT

# ShortLink

ShortLink is a small URL-shortening API backed by PostgreSQL. It creates cryptographically random seven-character Base62 codes, redirects to saved URLs, and exposes health and Prometheus metrics endpoints.

## Endpoints

| Method | Path | Behavior |
| --- | --- | --- |
| `POST` | `/links` | Create a short link; accepts an absolute HTTP or HTTPS URL up to 2048 characters. Returns `201 Created` with the link data, or `400` problem details for invalid input. |
| `GET` | `/{code}` | Redirect to the saved URL with `302 Found`, or return `404 Not Found`. Codes are seven Base62 characters. |
| `GET` | `/health/live` | Returns `200 OK` while the process is running. |
| `GET` | `/health/ready` | Returns `200 OK` when PostgreSQL is reachable, otherwise `503 Service Unavailable`. |
| `GET` | `/metrics` | Exposes Prometheus HTTP metrics and the link counters. |

Example requests (assuming the API is listening at `http://localhost:8080`):

```sh
curl -i -X POST http://localhost:8080/links \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com/some/long/path"}'

curl -i http://localhost:8080/Ab12Cd3
curl -i http://localhost:8080/health/live
curl -i http://localhost:8080/health/ready
curl -s http://localhost:8080/metrics
```

The create response includes `code`, `shortUrl`, `longUrl`, and `createdAt`. Set `BASE_URL` to the public base URL used for generated short links. Prometheus metrics include `shortlink_links_created_total` and `shortlink_redirects_total{result="hit|miss"}`.

## Configuration

| Environment variable | Default | Description |
| --- | --- | --- |
| `ConnectionStrings__Default` | Required | PostgreSQL connection string used by EF Core. |
| `BASE_URL` | `http://localhost:8080` | Base URL prepended to generated short codes. |
| `RUN_MIGRATIONS` | `false` | Set to `true` to apply EF Core migrations at startup. |
| `ASPNETCORE_URLS` | `http://+:8080` | ASP.NET Core listening URL. |

Settings can also be supplied through the normal ASP.NET Core `appsettings.json` configuration providers. Logs are structured JSON written to stdout.

## Run locally

Install the .NET 10 SDK and have PostgreSQL 17 available. For a disposable local database, start PostgreSQL with:

```sh
docker run --name shortlink-postgres -e POSTGRES_PASSWORD=shortlink -e POSTGRES_DB=shortlink -p 5432:5432 -d postgres:17
```

From the repository root, configure the connection and let the application create/update its schema:

```sh
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=shortlink;Username=postgres;Password=shortlink"
$env:RUN_MIGRATIONS = "true"
dotnet run --project src/ShortLink.Api
```

On shells other than PowerShell, set the same environment variables using that shell's syntax. Set `RUN_MIGRATIONS=false` when schema changes are managed separately.

## Run tests

From the repository root:

```sh
dotnet test
```

The integration tests use EF Core's in-memory provider and do not require a running PostgreSQL instance; the readiness failure test uses an intentionally unreachable local PostgreSQL port.

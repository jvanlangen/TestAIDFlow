# TestAIDFlow

This repository contains the household meal and grocery planner defined in
[`docs/aidflow/`](docs/aidflow/README.md), built with the approved React/Vite,
ASP.NET Web API, PostgreSQL and Docker Compose design.

## Run on the household network

1. Install Docker Engine and the Docker Compose plugin on the household server.
2. Copy `.env.example` to `.env`. Set `AIDFLOW_HOST_ADDRESS` to the server's
   fixed household-LAN IP address, and set a private PostgreSQL password.
   The application will not start with the example's empty values.
3. Start the application with `docker compose up -d --build`.
4. Open `http://<AIDFLOW_HOST_ADDRESS>:8080` from a household device.

The web/API service binds only to the configured server address. PostgreSQL has
no published host port and is available only to the application over the
Compose network. The named `postgres-data` volume preserves data when
containers are recreated or the server restarts.

The first version has no authentication. Do not forward the application port
from the internet; restrict access to the household network with the server and
router firewall. LAN transport protection has not been configured. Persistent
storage is not a backup; automated backup and recovery are not implemented.

## Development checks

- Frontend: `npm ci --prefix src/Web`, `npm run build --prefix src/Web`, and
  `npm run lint --prefix src/Web`.
- API: `dotnet build src/Api/Api.csproj`.
- API unit tests: `dotnet test tests/Api.Tests/Api.Tests.csproj`.

The API applies checked-in EF Core migrations at startup. The frontend is built
into the API image and served from the same origin as its `/api` endpoints.

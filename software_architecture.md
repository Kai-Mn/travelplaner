# Abstract 
Modal verbs in this doccumentation are in accordance with [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119)

# 1. Introduction & Goals

### 1.1 Requirements Overview
The system provides an easy way for a user to plan their vacation and share it with friends. 

### 1.2 Quality Goals
1) **Maintainabilety** a midlevel engineer should find the structure familiar.
 Setting up local dev env for an engineer shouldnt take longer than 20 min.
2) **Usability** new user should be able to plan a daytrip intuetively in 5min
3) **Security & Privacy**  GDPR compliant, client data encrypted in transit and rest

### 1.2 Stakeholders

| Stakeholder | Role/Interest | What they need               |
| ----------- | ------------- | ---------------------------- |
| Traveler    | End user      | simple ux, reliabilety       |
| Developer   | Implementers  | Clear structure, constraints |

# 2. Constraints
### 2.1 Technicall constraints
* **Hosting** must be selfhostable with a local database
* **Database technology** must support **SQLite** and **Postgress**
* **Identity & access management** must support account creation with just email + pw, should support SSO (OIDC) via </insert provider>
* **Integration standards** external communication must be via **REST/JSON** 
* **Client environment** Web client musst support latest two versions of Firefox and Chrome. Design must be responsive and be usable on a mobile client
* **Programing Languages** backend must be implemented **C# / .NET 10**. Frontend must be **Typescritp / Angular 20**
* **Dev/Ops** must have **CI/CD gates** and an automatic deployment pipeline


### 2.2 Organizational constraints
none

### 2.3 Legal / regulatory constraints
to be determined if neccessary

### 2.4 Financial constraints
none

### 2.5 Time constraints
* **MVP** should be live by 30.04.2026

# 3. Context and Scope

## 3.1 Business Context (external communication partners)
| Communication Partner            | Type                 | Role / Exchanged Information                                                                      | Interface / Protocol                                             | Direction      | Frequency / Timing                     |
| -------------------------------- | -------------------- | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- | -------------- | -------------------------------------- |
| End User (Traveler)              | Human actor          | Marks points of interest (POIs), creates/edits a route based on selected POIs; receives computed route and saved plan overview | Web UI (HTTPS)                                                   | Bi-directional | On demand                              |
| SSO Service (Identity Provider)  | External system      | Authentication, authorization claims (user id, roles), token issuance/validation                  | OIDC/OAuth2 over HTTPS | Bi-directional | At login + token refresh               |
Persistence Backend (modular, one of the options below)   | External system (DB/service) | Stores/reads routes, POIs, user-related data | See “Persistence options” table | Bi-directional | High / per request

### 3.2 Persistence options (exactly one active per deployment)
| Persistence Option       | Type                 | Purpose / Stored Data                  | Interface / Protocol                                  | Notes                                                                     |
|--------------------------|----------------------|----------------------------------------|-------------------------------------------------------|---------------------------------------------------------------------------|
| Local SQLite DB          | Local DB (embedded)  | Routes, POIs, user-related data | SQLite file access via SQLite driver (SQL)            | Single-node; typically for local/dev/offline use or selfhosting                      |
| PostgreSQL DB            | DB server            | Routes, POIs, user-related data        | PostgreSQL wire protocol over TCP via DB driver (SQL) | Typical production setup; supports concurrency and scaling                |
| Web DB (DBaaS / REST DB) | External web service | Routes, POIs, user-related data        | HTTPS REST/JSON (or GraphQL)                          | Requires stable API contract; latency and availability depend on provider |

### 3.3 Technical Context 
| Topic                     | Example Description                                                                                                                                           |
|---------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Modularity approach       | TravelPlanner uses a persistence interface/port (e.g., RouteRepository, PoiRepository) with pluggable adapters: SQLiteAdapter, PostgresAdapter, WebDbAdapter. |
| Configuration / selection | Selected via deployment configuration (env vars/config file). Only one adapter is active at runtime per deployment.                                           |
| Contract compatibility    | All adapters implement the same repository contracts and data model; migrations/versioning ensure schema/API compatibility across options.                    |


# 4. Solution Strategy

### 4.1 Summary of fundamental decisions

The TravelPlanner system is implemented as a **.NET 10 / C#** backend following **Clean Architecture** to enforce separation of concerns, testability, and interchangeable infrastructure (notably the modular persistence backends: **SQLite**, **PostgreSQL**, or **Web DB**). The backend exposes an **OpenAPI-described HTTP API**; the frontend is **generated/typed from OpenAPI** to avoid manual DTO/service drift.

Authentication supports two modes:  
1) **SSO via OIDC Authorization Code + PKCE **handled by Angular, calling the backend with Bearer tokens (backend validates JWTs).   
2) **Local accounts (email/password)** via  two custom backend API endpoints (simpler to implement), issuing tokens that the frontend uses similarly for API access. 

The **Angular 21** frontend uses **Signals** and **NgRx Signal Stores** to centralize state and API communication, with a component-based using **PrimeNG** components, UI styled via **Tailwind CSS**.

Local development and test execution must be reproducible; therefore the system is designed to run locally with **Docker + docker-compose**, while still allowing a **SQLite-only**  mode where appropriate.

### 4.2 Technology decisions

* **Backend runtime/framework**: .NET 10, C#
* **Backend architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation)
* **API style**: REST over HTTPS, described via OpenAPI
* **Persistence:** modular adapter approach (SQLite / PostgreSQL / Web DB)
* **ORM**: Entity Framework Core (where DB is relational); Web DB via HTTP client adapter
* **Mapping**: AutoMapper for object-to-object mapping
* **Controller/application interaction:** MediatR with Commands/Queries (CQRS style)
* **Frontend**: Angular 21, Signals
* **Frontend state**: NgRx Signal Store; API calls live in stores
* **Styling**: Tailwind CSS + PrimeNg
* **Local dev**: Docker + docker-compose (and optionally SQLite without containers)
  
### 4.3 Top-level decomposition / patterns

* **Clean Architecture** to isolate:
    * **Domain**: entities/value objects, domain rules
    * **Application**: use cases (MediatR handlers), ports/interfaces, DTO shaping
    * **Infrastructure**: EF Core implementations, SSO/OIDC integration, Web DB adapter
    * **Presentation**: Web API controllers, OpenAPI surface
* **CQRS with MediatR:** controllers are thin; logic in command/query handlers.
* **Repository/Port-Adapter** approach to support either/or persistence backend (SQLite vs Postgres vs Web DB) with minimal code changes.
* **DTO boundary**: frontend never consumes domain entities directly; API contracts are DTOs.


### 4.4 Key quality goals → solution approaches
| **Quality goal**          | **Scenario**                                                                            | **Solution approach**                                                                                                                       | **Link to Details**                                    |
|---------------------------|-----------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------|
| Testability               | Run unit/integration tests locally without external dependencies                        | Clean Architecture + dependency inversion; MediatR handlers unit-testable; persistence via interfaces; docker-compose for integration tests |  |
| Maintainability           | Add a new feature (e.g., route optimization rule) without touching UI/persistence       | Use case logic in Application layer; thin controllers; mapping isolated via AutoMapper profiles                                             | |
| Modularity of persistence | Switch deployment from SQLite to PostgreSQL (or Web DB) without changing business logic | Persistence adapters behind repository interfaces; configuration selects implementation                                                     |  |
| API contract consistency  | Prevent frontend/backend drift and manual DTO duplication                               | OpenAPI-first (or OpenAPI-generated) DTOs/services for frontend; DTOs form stable contract                                                  | |
| Security                  | User logs in via SSO (PKCE) or via email/password and can call protected endpoints | Two auth modes: (1) validate OIDC JWT Bearer tokens from SSO; (2) custom /auth/* endpoints issue backend-signed tokens; backend enforces authorization via claims/roles/scopes |  |
| Local reproducibility     | New developer can run system locally with minimal setup                                 | Docker + docker-compose for dependencies; option for SQLite local mode                                                                      |  |


# 5. Building Block View
none

# 6. Runtime View
none

# 7. Deployment View 

### 7.1 Infrastructure Level 1 (overview) / Deployment options
The TravelPlanner system supports two deployment modes selected at startup via environment variables:
     
1) **Single-container mode (SQLite)**
    * One Docker container runs the backend API and serves the Angular SPA.
    * SQLite is stored inside the container (or optionally on a mounted volume).
     

2) **Compose mode (PostgreSQL)**
    * One Docker container runs the backend API and serves the Angular SPA.
    * A separate Docker container runs PostgreSQL.
        Backend connects to PostgreSQL over the Docker network.

### 7.2 Infrastructure diagram (textual)

Mode A: **Single container + SQLite**

    Node: docker-host
        Container: travelplanner-app
            Components: Angular static files (served), .NET API
            Data: SQLite database (file)

Mode B: **docker-compose + PostgreSQL**

    Node: docker-host
        Container: travelplanner-app
            Components: Angular static files (served), .NET API
        Container: travelplanner-postgres
            Component: PostgreSQL DB
        Network: travelplanner-net (internal)

### 7.2 Configuration / environment variables (startup selection)

The persistence backend is selected at application startup.

Variables:\
`PERSISTENCE_PROVIDER` = `sqlite` | `postgres`\
`SQLITE__PATH` = `/data/travelplanner.db` (for SQLite mode)\
`CONNECTIONSTRINGS__POSTGRES` = `Host=travelplanner-postgres;Port=5432;Database=travelplanner;Username=...;Password=...` (for Postgres mode)

Auth-related variables :\
`AUTH_SSO_AUTHORITY` = `https://sso.example.com/realms/...`\
`AUTH_SSO_AUDIENCE` (CLIENT_ID) = `travelplanner-api`\
`AUTH_LOCAL_JWT_SIGNING_KEY `(or certificate reference)

### 7.3 CI/CD (GitHub pipelines) — release deployment
**Goal**: Build, test, publish a Docker image and deploy it on demand.

**Pipeline stages**:

**Build & Test**
   * Restore, build, run backend tests
   * (Optional) run frontend tests/lint

**Generate OpenAPI + Generate Angular client**
   * Ensure contract generation is part of build and consistent

**Build Docker image**
   * travelplanner-app:\<version\>

**Publish image**
   * Push to a container registry (ghcr.io)

**Manual approval / dispatch deploy**
   * “Push of a button” via GitHub Actions workflow_dispatch (or environment approvals)

**Deploy**
   * SSH into target host and run:
      * SQLite mode: docker run ...
      * Postgres mode: docker compose pull && docker compose up -d
   * Or equivalent remote deployment mechanism.
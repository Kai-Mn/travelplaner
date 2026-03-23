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
1) **SSO via OIDC Authorization Code + PKCE** handled by Angular, calling the backend with Bearer tokens (backend validates JWTs).   
2) **Local accounts (email/password)** via  two custom backend API endpoints (simpler to implement), issuing tokens that the frontend uses similarly for API access. 

The **Angular 21** frontend uses **Signals** and **NgRx Signal Stores** to centralize state and API communication, with a component-based using **PrimeNG** components, UI styled via **Tailwind CSS**.

Local development and test execution must be reproducible; therefore the system is designed to run locally with **Docker + docker-compose**, while still allowing a **SQLite-only**  mode where appropriate.

### 4.2 Technology decisions

* **Backend runtime/framework**: .NET 10, C#
* **Backend architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation)
* **API style**: REST over HTTPS, described via OpenAPI
* **Persistence:** modular adapter approach (SQLite / PostgreSQL / Web DB)
* **ORM**: Entity Framework Core (where DB is relational); Web DB via HTTP client adapter
* **Mapping**: Mapster for object-to-object mapping
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
| Maintainability           | Add a new feature (e.g., route optimization rule) without touching UI/persistence       | Use case logic in Application layer; thin controllers; mapping isolated via Mapster profiles                                             | |
| Modularity of persistence | Switch deployment from SQLite to PostgreSQL (or Web DB) without changing business logic | Persistence adapters behind repository interfaces; configuration selects implementation                                                     |  |
| API contract consistency  | Prevent frontend/backend drift and manual DTO duplication                               | OpenAPI-first (or OpenAPI-generated) DTOs/services for frontend; DTOs form stable contract                                                  | |
| Security                  | User logs in via SSO (PKCE) or via email/password and can call protected endpoints | Two auth modes: (1) validate OIDC JWT Bearer tokens from SSO; (2) custom /auth/* endpoints issue backend-signed tokens; backend enforces authorization via claims/roles/scopes |  |
| Local reproducibility     | New developer can run system locally with minimal setup                                 | Docker + docker-compose for dependencies; option for SQLite local mode                                                                      |  |


# 5. Building Block View
# 5. Building Block View

## 5.1 Level 1 — System White-Box (Overall System)

The TravelPlanner system is decomposed into two top-level building blocks — the **Angular SPA (Frontend)** and the **.NET Backend API** — plus the external systems they interact with. The backend follows **Clean Architecture** with four layers (Domain, Application, Infrastructure, Presentation).

### 5.1.1 Overview Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          TravelPlanner System                               │
│                                                                             │
│  ┌──────────────────────┐        HTTPS/JSON        ┌─────────────────────┐  │
│  │                      │ ◄──────────────────────►  │                     │  │
│  │   Angular SPA        │                           │   .NET Backend API  │  │
│  │   (Frontend)         │                           │                     │  │
│  │                      │                           │                     │  │
│  └──────────┬───────────┘                           └──────┬──────────────┘  │
│             │                                              │                 │
└─────────────┼──────────────────────────────────────────────┼─────────────────┘
              │                                              │
              │ OIDC/OAuth2                                  │ SQL / HTTPS
              ▼                                              ▼
   ┌─────────────────────┐                      ┌────────────────────────┐
   │  SSO Identity        │                      │  Persistence Backend   │
   │  Provider (OIDC)     │                      │  (SQLite / PostgreSQL  │
   │                      │                      │   / Web DB)            │
   └─────────────────────┘                      └────────────────────────┘
```

### 5.1.2 Building Block Catalogue — Level 1

| Building Block | Responsibility | Technology | Interfaces |
|---|---|---|---|
| **Angular SPA (Frontend)** | User interface: map navigation, location/journey management, authentication flows (SSO + local). Communicates with backend via generated OpenAPI client. | Angular 21, Signals, NgRx Signal Store, PrimeNG, Tailwind CSS | Consumes: REST/JSON API (Backend). Initiates: OIDC Authorization Code + PKCE flow (SSO Provider). |
| **.NET Backend API** | Business logic, use-case orchestration, authentication/authorization, persistence coordination, OpenAPI surface. | .NET 10, C#, Clean Architecture, MediatR, EF Core, Mapster | Exposes: REST/JSON API (OpenAPI). Consumes: Persistence Backend (SQL or HTTPS). Validates: JWT Bearer tokens (SSO or self-issued). |
| **SSO Identity Provider** *(external)* | Authenticates users via OIDC, issues JWT access/ID tokens, provides user claims. | Any OIDC-compliant IdP | OIDC/OAuth2 over HTTPS |
| **Persistence Backend** *(external, modular)* | Stores and retrieves all domain data (users, locations, journeys, images, tags). Exactly one option active per deployment. | SQLite / PostgreSQL / Web DB (DBaaS) | SQLite file access, PostgreSQL wire protocol, or HTTPS REST/JSON |

### 5.1.3 Important Interfaces — Level 1

| Interface | Description | Protocol / Format |
|---|---|---|
| **Frontend ↔ Backend API** | All user-initiated operations (CRUD for locations, journeys, auth). Contract defined by OpenAPI spec; Angular client is generated from it. | HTTPS, REST/JSON |
| **Frontend → SSO Provider** | OIDC Authorization Code + PKCE login flow. Frontend redirects user, receives tokens. | OIDC/OAuth2 over HTTPS |
| **Backend → Persistence** | Read/write domain data. Abstracted behind repository interfaces; adapter selected at startup. | SQL (EF Core) or HTTPS REST/JSON |
| **Backend → SSO Provider** | JWT validation (fetching JWKS/metadata to validate Bearer tokens). | HTTPS (OIDC discovery) |

---

## 5.2 Level 2 — Frontend White-Box (Angular SPA)

### 5.2.1 Overview Diagram (textual)

```
┌──────────────────────────────────────────────────────────────────┐
│                        Angular SPA                               │
│                                                                  │
│  ┌──────────────────┐   ┌──────────────────┐                    │
│  │  Auth Module      │   │  Map Module       │                   │
│  │  - Login Page     │   │  - Map View       │                   │
│  │  - Register Page  │   │  - Map Component  │                   │
│  │  - Auth Store     │   │                   │                   │
│  │  - Auth Guard     │   │                   │                   │
│  └──────────────────┘   └──────────────────┘                    │
│                                                                  │
│  ┌──────────────────┐   ┌──────────────────┐                    │
│  │  Location Module  │   │  Journey Module   │                   │
│  │  - Location Detail│   │  - Journey List   │                   │
│  │  - Location Form  │   │  - Journey Detail │                   │
│  │  - Location Store │   │  - Journey Form   │                   │
│  │  - Tag Component  │   │  - Journey Store  │                   │
│  │  - Image Upload   │   │                   │                   │
│  └──────────────────┘   └──────────────────┘                    │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Shared / Core                                            │   │
│  │  - Generated OpenAPI Client (API Services + DTOs)         │   │
│  │  - HTTP Interceptors (Bearer token injection)             │   │
│  │  - Layout / Shell Component                               │   │
│  └──────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
```

### 5.2.2 Building Block Catalogue — Level 2 (Frontend)

| Building Block | Responsibility | Fulfilled Requirements |
|---|---|---|
| **Auth Module** | Account creation (email + password), login (local + SSO/OIDC PKCE), token management, route guards for protected pages. | *"As a visitor I want to create an account with email and password."* |
| **Map Module** | Renders an interactive map, allows navigation (pan, zoom), displays location markers, handles click-to-mark interactions. | *"As a visitor/user I want to navigate a map."*, *"As a user I want to mark locations on this map."* |
| **Location Module** | CRUD UI for locations: add/edit text descriptions, upload images, manage tags, delete locations. Communicates via Location Store → OpenAPI client. | *"As a user I want to add a text description / pictures / tags to locations."*, *"As a user I want to delete a location and all its associated data."* |
| **Journey Module** | CRUD UI for journeys: create journey, assign locations to a journey, view journey details, delete journeys. Communicates via Journey Store → OpenAPI client. | *"As a user I want to group locations into journeys."*, *"As a user I want to delete journeys."* |
| **Shared / Core** | Generated OpenAPI TypeScript client (API services + DTO types), HTTP interceptors (attach Bearer token to requests), app shell/layout, shared UI components. | Cross-cutting; supports API contract consistency and security. |

### 5.2.3 Important Interfaces — Level 2 (Frontend)

| Interface | From → To | Description |
|---|---|---|
| **NgRx Signal Stores → OpenAPI Client** | Each feature store → Generated API services | Stores encapsulate all API calls; components read signals from stores. |
| **Auth Interceptor → HTTP Client** | Core → Angular HttpClient | Injects Bearer token into every outgoing API request. |
| **Auth Guard → Router** | Auth Module → Angular Router | Protects routes; redirects unauthenticated users to login. |

---

## 5.3 Level 2 — Backend White-Box (.NET Backend API)

The backend follows **Clean Architecture** with four layers. Dependencies point inward: Presentation → Application → Domain; Infrastructure → Application → Domain.

### 5.3.1 Overview Diagram (textual)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         .NET Backend API                                │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  Presentation Layer                                               │  │
│  │  ┌─────────────────┐ ┌──────────────────┐ ┌────────────────────┐  │  │
│  │  │ AuthController  │ │LocationController│ │ JourneyController  │  │  │
│  │  └─────────────────┘ └──────────────────┘ └────────────────────┘  │  │
│  │  ┌─────────────────┐ ┌──────────────────────────────────────┐     │  │
│  │  │ ImageController │ │ OpenAPI / Swagger Middleware         │     │  │
│  │  └─────────────────┘ └──────────────────────────────────────┘     │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                              │ MediatR                                  │
│                              ▼                                          │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  Application Layer                                                │  │
│  │  ┌───────────────────────┐  ┌───────────────────────┐             │  │
│  │  │ Auth Use Cases        │  │ Location Use Cases    │             │  │
│  │  │ - RegisterCommand     │  │ - CreateLocationCmd   │             │  │
│  │  │ - LoginCommand        │  │ - UpdateLocationCmd   │             │  │
│  │  │                       │  │ - DeleteLocationCmd   │             │  │
│  │  │                       │  │ - GetLocationQuery    │             │  │
│  │  │                       │  │ - GetLocationsQuery   │             │  │
│  │  └───────────────────────┘  └───────────────────────┘             │  │
│  │  ┌───────────────────────┐  ┌───────────────────────┐             │  │
│  │  │ Journey Use Cases     │  │ Image Use Cases       │             │  │
│  │  │ - CreateJourneyCmd    │  │ - UploadImageCmd      │             │  │
│  │  │ - DeleteJourneyCmd    │  │ - DeleteImageCmd      │             │  │
│  │  │ - AddLocationToJrnyCmd│  │ - GetImagesQuery      │             │  │
│  │  │ - RemoveLocFromJrnyCmd│  │                       │             │  │
│  │  │ - GetJourneyQuery     │  │                       │             │  │
│  │  │ - GetJourneysQuery    │  │                       │             │  │
│  │  └───────────────────────┘  └───────────────────────┘             │  │
│  │  ┌───────────────────────┐  ┌───────────────────────┐             │  │
│  │  │ Tag Use Cases         │  │ Port Interfaces       │             │  │
│  │  │ - AddTagCmd           │  │ - ILocationRepository │             │  │
│  │  │ - RemoveTagCmd        │  │ - IJourneyRepository  │             │  │
│  │  │ - GetTagsQuery        │  │ - IUserRepository     │             │  │
│  │  │                       │  │ - IImageStore         │             │  │
│  │  │                       │  │ - IAuthService        │             │  │
│  │  └───────────────────────┘  └───────────────────────┘             │  │
│  │  ┌───────────────────────┐                                        │  │
│  │  │ DTOs & Mapping        │                                        │  │
│  │  │ - Mapster Profiles    │                                        │  │
│  │  └───────────────────────┘                                        │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                              │ Interfaces                               │
│                              ▼                                          │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  Domain Layer                                                     │  │
│  │  ┌─────────────┐ ┌────────────┐ ┌──────────┐ ┌─────────────────┐  │  │
│  │  │ User        │ │ Location   │ │ Journey  │ │ Value Objects   │  │  │
│  │  │ (Entity)    │ │ (Entity)   │ │ (Entity) │ │ - Tag           │  │  │
│  │  │             │ │ - Images   │ │          │ │ - Coordinates   │  │  │
│  │  │             │ │ - Tags     │ │          │ │ - Description   │  │  │
│  │  │             │ │ - Coords   │ │          │ │                 │  │  │
│  │  └─────────────┘ └────────────┘ └──────────┘ └─────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────────────────┐ │  │
│  │  │ Domain Rules (e.g., location must have coords, journey       │ │  │
│  │  │ must have at least a name, cascading delete rules)           │ │  │
│  │  └──────────────────────────────────────────────────────────────┘ │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                              ▲ Implements interfaces                    │
│                              │                                          │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  Infrastructure Layer                                             │  │
│  │  ┌───────────────────────┐  ┌───────────────────────┐             │  │
│  │  │ Persistence Adapters  │  │ Auth Infrastructure   │             │  │
│  │  │ - EF Core DbContext   │  │ - Local Auth Service  │             │  │
│  │  │ - SQLite Adapter      │  │   (password hashing,  │             │  │
│  │  │ - PostgreSQL Adapter  │  │    JWT issuance)      │             │  │
│  │  │ - WebDb Adapter       │  │ - OIDC JWT Validation │             │  │
│  │  │   (HTTP client)       │  │                       │             │  │
│  │  └───────────────────────┘  └───────────────────────┘             │  │
│  │  ┌───────────────────────┐  ┌───────────────────────┐             │  │
│  │  │ Image Storage         │  │ Configuration /       │             │  │
│  │  │ - File System Store   │  │ DI Registration       │             │  │
│  │  │   (or blob adapter)   │  │ - Adapter selection   │             │  │
│  │  │                       │  │   at startup          │             │  │
│  │  └───────────────────────┘  └───────────────────────┘             │  │
│  └───────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.3.2 Building Block Catalogue — Level 2 (Backend)

| Building Block | Responsibility | Fulfilled Requirements |
|---|---|---|
| **Presentation Layer** | Thin REST controllers that receive HTTP requests, delegate to MediatR commands/queries, and return DTOs. Hosts OpenAPI/Swagger middleware for contract generation. | Cross-cutting: exposes all user-facing operations as API endpoints. |
| **Application Layer** | Contains all use-case logic as MediatR command/query handlers. Defines port interfaces (repositories, services). Shapes DTOs and Mapster profiles. No dependency on infrastructure. | All user stories are orchestrated here (create account, CRUD locations, CRUD journeys, manage images/tags). |
| **Domain Layer** | Core entities (`User`, `Location`, `Journey`), value objects (`Tag`, `Coordinates`, `Description`), and domain rules. No external dependencies. | Encodes business invariants (e.g., a location must have coordinates; deleting a location cascades to images/tags). |
| **Infrastructure Layer** | Implements port interfaces. Contains EF Core DbContext, persistence adapters (SQLite, PostgreSQL, WebDb), auth services (local JWT issuance, OIDC validation), image storage. | *"As a user I want the location and its associated data to be persisted."* Supports modular persistence and dual auth modes. |

### 5.3.3 Important Interfaces — Level 2 (Backend)

| Interface | Defined In | Implemented In | Description |
|---|---|---|---|
| `ILocationRepository` | Application | Infrastructure (EF Core / WebDb Adapter) | CRUD operations for Location entities including associated images and tags. |
| `IJourneyRepository` | Application | Infrastructure (EF Core / WebDb Adapter) | CRUD operations for Journey entities and their location associations. |
| `IUserRepository` | Application | Infrastructure (EF Core / WebDb Adapter) | User lookup, creation, credential storage. |
| `IImageStore` | Application | Infrastructure (File System / Blob Adapter) | Store, retrieve, and delete image binary data associated with locations. |
| `IAuthService` | Application | Infrastructure (Local Auth Service) | Register user (email + password), authenticate, issue JWT tokens. |

---

## 5.4 Level 3 — Domain Entities (White-Box)

This level details the core domain model and the relationships between entities.

### 5.4.2 Entity Descriptions

| Entity / Value Object | Attributes | Description | Fulfilled Requirements |
|---|---|---|---|
| **User** | Id, Email, PasswordHash, CreatedAt | Represents a registered user. Owns locations and journeys. | *"As a visitor I want to create an account with email and password."* |
| **Location** | Id, Coordinates (lat/lng), Description, CreatedAt, UserId (FK) | A point of interest marked on the map by a user. Aggregate root for its images and tags. | *"As a user I want to mark locations on the map."*, *"As a user I want to add a text description."*, *"As a user I want the location and its data to be persisted."* |
| **Image** | Id, FilePath/BlobRef, LocationId (FK) | A picture associated with a location. Deleted when the parent location is deleted. | *"As a user I want to add pictures to locations."*, *"As a user I want to delete a location and all its associated data."* |
| **Tag** | Id, Name, LocationId (FK) | A label/tag attached to a location. Deleted when the parent location is deleted. | *"As a user I want to add a tag to locations."*, *"As a user I want to delete a location and all its associated data."* |
| **Journey** | Id, Name, Description, CreatedAt, UserId (FK) | A named grouping of locations representing a trip/plan. Has a many-to-many relationship with locations. | *"As a user I want to group locations into journeys."*, *"As a user I want to delete journeys."* |
| **Coordinates** *(Value Object)* | Latitude, Longitude | Immutable geographic position of a location. | *"As a user I want to mark locations on the map."* |

### 5.4.3 Relationships

| Relationship | Type | Description |
|---|---|---|
| User → Location | 1 : * | A user owns zero or more locations. |
| User → Journey | 1 : * | A user owns zero or more journeys. |
| Location → Image | 1 : * | A location has zero or more images. Cascade delete. |
| Location → Tag | 1 : * | A location has zero or more tags. Cascade delete. |
| Journey ↔ Location | * : * | A journey groups multiple locations; a location can belong to multiple journeys. Implemented via a join table (`JourneyLocation`). |

---


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

## 8. rosscutting Concepts
These concerns span multiple building blocks and are not isolated to a single module.

| Concern | Approach | Affected Building Blocks |
|---|---|---|
| **Authentication & Authorization** | Dual-mode: (1) OIDC/PKCE via external IdP, (2) local email+password with backend-issued JWT. Backend validates Bearer tokens on every request. Frontend attaches tokens via HTTP interceptor. | Auth Module (FE), AuthController (BE), Auth Infrastructure (BE), Auth Guard (FE) |
| **OpenAPI Contract** | Backend generates OpenAPI spec; frontend TypeScript client is auto-generated from it. Ensures type-safe, drift-free communication. | Presentation Layer (BE), Shared/Core (FE) |
| **Persistence Abstraction** | Repository interfaces in Application layer; adapters in Infrastructure layer. Adapter selected at startup via environment variable. | Application Layer ports, Infrastructure Layer adapters |
| **Error Handling** | Consistent error response format (RFC 7807 Problem Details). Application layer throws domain exceptions; Presentation layer maps them to HTTP status codes. | All layers |
| **Mapping** | Mapster profiles convert between Domain entities, Application DTOs, and API response models. | Application Layer, Presentation Layer |
| **Cascading Deletes** | Deleting a location removes all associated images (including stored files) and tags. Deleting a journey removes the journey and its location associations (but not the locations themselves). | Domain rules, Repository implementations |
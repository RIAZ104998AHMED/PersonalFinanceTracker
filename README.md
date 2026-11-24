# Personal Finance Tracker

Student: Riaz Ahmed  
Course: Backend Development  
University: HITS, Tomsk State University  


# Personal Finance Tracker API

ASP.NET Core Web API for managing personal finances, including accounts, categories, operations, transfers, statistics, authentication, and refresh tokens.

## Technologies

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- JWT authentication
- Swagger / OpenAPI
- xUnit

## Project Structure

The solution contains the following projects:

- `PersonalFinanceTracker.Api` — API controllers, authentication configuration, exception handling, Swagger and OpenAPI.
- `PersonalFinanceTracker.BusinessLogic` — services, DTOs, validation and business rules.
- `PersonalFinanceTracker.DataAccess` — Entity Framework Core entities, DbContext, migrations and database seeding.
- `PersonalFinanceTracker.Tests` — unit tests.

## Configuration

The application requires a PostgreSQL connection string and JWT signing key.

Sensitive values should be stored in User Secrets and must not be committed to Git.

Example:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=finance_tracker;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Jwt:Key" "YOUR_JWT_SECRET_KEY"
```

The configured PostgreSQL database is created and migrated automatically when the application starts.

## Run the Application

From the solution root directory:

```powershell
dotnet run --project PersonalFinanceTracker.Api/PersonalFinanceTracker.Api.csproj
```

Alternatively, run the `PersonalFinanceTracker.Api` project directly from Visual Studio.

The application is available locally at:

```text
http://localhost:5126
```

## OpenAPI and Swagger

The OpenAPI document is available at:

```text
http://localhost:5126/openapi/v1.json
```

Swagger UI is available at:

```text
http://localhost:5126/swagger
```

Protected endpoints require JWT authentication.

To authorize in Swagger:

1. Call `POST /api/auth/login`.
2. Copy the returned `accessToken`.
3. Click **Authorize** in Swagger.
4. Paste the access token.
5. Click **Authorize**.

## Demo User

The database seeder creates the following demo user:

```text
Email: demo@fintracker.local
Password: Demo12345
```

This account can be used for testing the API after creating a fresh database.

## Authentication

The API supports:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

All endpoints except registration, login and refresh require a valid access token where applicable.

## Main API Endpoints

### Accounts

```text
GET    /api/accounts
POST   /api/accounts
GET    /api/accounts/{id}
PUT    /api/accounts/{id}
PATCH  /api/accounts/{id}/archive
DELETE /api/accounts/{id}
```

### Categories

```text
GET    /api/categories
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
```

### Operations

```text
GET    /api/operations
POST   /api/operations
GET    /api/operations/{id}
PUT    /api/operations/{id}
DELETE /api/operations/{id}
```

### Transfers

```text
GET    /api/transfers
POST   /api/transfers
GET    /api/transfers/{id}
DELETE /api/transfers/{id}
```

### Statistics

```text
GET /api/statistics
```

### Current User

```text
GET /api/users/me
PUT /api/users/me
```

## Database Migrations

To create a migration from the solution root:

```powershell
dotnet ef migrations add MigrationName --project PersonalFinanceTracker.DataAccess/PersonalFinanceTracker.DataAccess.csproj --startup-project PersonalFinanceTracker.Api.csproj
```

To apply migrations:

```powershell
dotnet ef database update --project PersonalFinanceTracker.DataAccess/PersonalFinanceTracker.DataAccess.csproj --startup-project PersonalFinanceTracker.Api.csproj
```

## Unit Tests

Run all tests from the solution root:

```powershell
dotnet test
```

The project includes tests for major business rules such as account balance calculation, account deletion restrictions, currency-change restrictions, categories, operations and transfers.

## Build

To verify the complete solution:

```powershell
dotnet build
```

Then run:

```powershell
dotnet test
```

## HTTP Test File

`PersonalFinanceTracker.Api.http` contains example requests for authentication, accounts, categories, operations, transfers, statistics and validation.

The file uses named requests and tokens returned by login responses instead of hard-coded access tokens.

On a fresh database, run the authentication requests first so that required test users are registered before later requests are executed.

## Bonus Features

The following bonus features are implemented:

- **B1 — Refresh Tokens:** users can obtain a new access token using a valid refresh token through `POST /api/auth/refresh`.
- **B2 — Logout / Token Revocation:** refresh tokens can be revoked through `POST /api/auth/logout`.
- Refresh tokens are rotated after use and revoked tokens cannot be reused.
- Unit tests cover the main business rules and bonus functionality.
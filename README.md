# Personal Finance Tracker

Student: Riaz Ahmed  
Course: Backend Development  
University: HITS, Tomsk State University  


# Personal Finance Tracker API

Personal Finance Tracker is an ASP.NET Core Web API developed for the Backend Development 2026 course.

The application provides authentication, accounts, categories, income and expense operations, transfers, account balance calculation, and statistics.

## Technology Stack

.NET 10  
ASP.NET Core Web API  
Entity Framework Core  
PostgreSQL  
ASP.NET Core Identity  
JWT Authentication  
OpenAPI / Swagger UI

## Solution Structure

The solution contains three projects:

```text
PersonalFinanceTracker.Api
PersonalFinanceTracker.BusinessLogic
PersonalFinanceTracker.DataAccess
```

`PersonalFinanceTracker.Api` contains controllers, API configuration, authentication configuration, Swagger/OpenAPI, and exception handling.

`PersonalFinanceTracker.BusinessLogic` contains DTOs, interfaces, services, validation, and business rules.

`PersonalFinanceTracker.DataAccess` contains entities, the Entity Framework Core DbContext, migrations, Identity storage, and database configuration.

## Database Configuration

The project uses PostgreSQL.

The default local database used during development is:

```text
Database: finance_tracker
PostgreSQL port: 5433
```

The database connection string should not be committed to the repository.

Configure it with .NET User Secrets from the API project:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=finance_tracker;Username=postgres;Password=YOUR_PASSWORD"
```

Configure the JWT signing key using User Secrets:

```powershell
dotnet user-secrets set "Jwt:Key" "YOUR_JWT_KEY_AT_LEAST_32_CHARACTERS_LONG"
```

The JWT issuer and audience are configured in `appsettings.json`.

The application runs Entity Framework Core migrations automatically during startup and seeds the required demonstration data when necessary.

## Running the Project

Restore the packages:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build
```

Run the API:

```powershell
dotnet run --project PersonalFinanceTracker.Api
```

When running through the current Visual Studio development profile, the HTTP base URL is:

```text
http://localhost:5126
```

Swagger UI is available in the Development environment at:

```text
http://localhost:5126/swagger
```

The generated OpenAPI document is available at:

```text
http://localhost:5126/openapi/v1.json
```

## Demonstration User

The seeded demonstration account is:

```text
Email: demo@fintracker.local
Password: Demo12345
```

The fixed demonstration user identifier is:

```text
11111111-1111-1111-1111-111111111111
```

Use the login endpoint to obtain a fresh JWT access token:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "demo@fintracker.local",
  "password": "Demo12345"
}
```

Protected endpoints require:

```text
Authorization: Bearer <accessToken>
```

## Implemented API

Authentication and profile functionality includes registration, login, reading the current user, and updating the current user's name.

Account functionality includes listing accounts, creating accounts, reading an account, updating an account, archiving and unarchiving an account, deleting an unused account, and calculating balances dynamically from operations and transfers.

Category functionality includes system and user categories, filtering by operation type, creation, renaming, deletion, duplicate-name validation, and protection of system categories.

Operation functionality includes creation, reading, updating, deletion, paging, date filtering, account filtering, operation-type filtering, and sorting by date or amount.

Transfer functionality includes creation, listing with paging, reading, and deletion. Transfers require two different accounts owned by the same user and using the same currency.

Statistics functionality supports statistics for either one account or all non-archived accounts of one currency. It provides total income, total expense, net value, category breakdown, category shares, and a daily or monthly timeline. Transfers are excluded from income and expense statistics.

## Business Rules

The implementation covers the core project rules R1–R16, including dynamic account balance calculation, amount validation, category/type matching, future-date validation, archived-account restrictions, account deletion and currency-change restrictions, transfer validation, category protections, unique e-mail addresses, password requirements, ownership checks, and statistics validation.

Errors are returned using ProblemDetails. Validation failures use validation error responses, and application exceptions are handled through the global exception handler rather than controller `try/catch` blocks.

## Statistics Verification

For the seeded demonstration data, the following request:

```http
GET /api/statistics?currency=RUB&from=2026-09-01&to=2026-09-30&groupBy=day
```

returns:

```text
Total income: 60000.00
Total expense: 53370.75
Net: 6629.25
```

For the demonstration Debit card account during August 2026:

```text
Total income: 60000.00
Total expense: 52040.75
Net: 7959.25
```

These values were verified against the required seed data.

## HTTP Request File

`PersonalFinanceTracker.Api.http` contains executable API requests for development and verification.

Before committing the repository, real JWT access tokens should be replaced with placeholders such as:

```text
@token = PASTE_TOKEN_HERE
@token2 = PASTE_SECOND_USER_TOKEN_HERE
@demoToken = PASTE_DEMO_TOKEN_HERE
```

A fresh token can be obtained through the login endpoint before testing protected endpoints.

## Bonuses
## Bonuses

### Refresh Tokens and Logout

The refresh-token and logout bonus functionality is implemented.

Implemented endpoints:

```text
POST /api/auth/refresh
POST /api/auth/logout
```

Refresh tokens are opaque random values. Only their SHA-256 hashes are stored in the database.

Each refresh token has an expiration time. When a refresh token is successfully used, the old token is revoked and replaced by a new refresh token.

Logout revokes the supplied refresh token so that it cannot be used again.

When refresh-token functionality is enabled, JWT access tokens expire after 15 minutes.

### Unit Tests

A separate xUnit project named `PersonalFinanceTracker.Tests` is included.

The tests use EF Core InMemory and cover the required business rules, including:

- R1 — account balance calculation
- R3 — category and operation type matching
- R5 — archived account restrictions
- R6 — deletion of accounts with operations/transfers
- R7 — account currency change restrictions
- R8 — transfer validation
- R10 — category uniqueness
- R12 — deletion of categories used by operations

Current test result:

```text
Total: 12
Passed: 12
Failed: 0
Skipped: 0
```

Run the tests with:

```powershell
dotnet test
```

### Unit Tests

Implemented an xUnit test project using EF Core InMemory.

The test suite contains 12 passing tests covering the required business rules, including R1, R3, R5, R6, R7, R8, R10 and R12.

Run the tests with:

```powershell
dotnet test

## Project Status

Week 6: completed  
Week 7: completed  
Week 8: completed  
Week 9: completed

The project is ready for Week 10 final corrections, API-contract verification, code freeze, and defense preparation.
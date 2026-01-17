# CrisilPreOnboardingApi (NET 8 + SQL Server)

## What this contains
- POST `/api/v1/pre-onboarding`
- Mandatory field validation with field-wise error responses
- SQL Server persistence via EF Core

## Configure
Update `appsettings.json`:
- ConnectionStrings:DefaultConnection
- Auth:ExpectedToken / Auth:ExpectedCompanyCode

## Run
In Visual Studio 2022:
- Open `CrisilPreOnboardingApiSolution.sln`
- Press F5
- Open Swagger at `/swagger`

## DB Migration (recommended)
Install EF tools and run:
- `dotnet tool install --global dotnet-ef`
- `dotnet ef migrations add InitialCreate`
- `dotnet ef database update`

## Sample Request (headers)
Token: 3546jb13m77mih25c4lldhswhqnxvzc4
CompanyCode: 3i


## Additional endpoints
- GET `/api/v1/pre-onboarding/{id}`
- GET `/api/v1/pre-onboarding?external_candidate_id=...&crisil_offer_id=...&from=YYYY-MM-DD&to=YYYY-MM-DD&page=1&pageSize=20`

## Global exception + request/response logging
- Unhandled exceptions return a standard JSON error with traceId.
- Request/response logging is enabled (best-effort, truncated to 20k chars).

## Audit fields
Entity includes: `CreatedUtc`, `UpdatedUtc`, `CreatedBy`, `UpdatedBy`, `RawRequestJson`.

> After extracting this version, please create a new EF migration to add the new columns.

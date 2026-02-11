# Contract Intelligence - Persistence Layer

This document describes the persistence foundation for the Contract Intelligence MVP built with .NET 9 and EF Core 9.

## Domain Entities

Located in `src/Domain/Entities/`:

### Contract
Core entity representing a contract with vendor, dates, renewal tracking, and risk scoring.

**Key Properties:**
- `Id` (Guid) - Primary key
- `Title` - Contract name
- `Vendor` - Vendor/supplier name
- `StartDate`, `EndDate` - Contract period
- `RenewalDate` - When renewal is due
- `RiskScore` - Calculated risk assessment (0-100)
- `Status` - Contract status
- Navigation properties to `Documents` and `Clauses`

### Document
Represents uploaded contract documents.

**Key Properties:**
- `Id` (Guid) - Primary key
- `ContractId` - Foreign key to Contract
- `FileName`, `FilePath` - File information
- `FileSize`, `MimeType` - File metadata
- `UploadedAt` - Upload timestamp
- Navigation properties to `Contract` and `Clauses`

### Clause
Represents extracted contract clauses with AI confidence scoring.

**Key Properties:**
- `Id` (Guid) - Primary key
- `ContractId` - Foreign key to Contract
- `DocumentId` - Optional foreign key to Document
- `ClauseType` - Type/category of clause
- `Excerpt` - Extracted text
- `Confidence` - AI extraction confidence (0-1)
- `PageNumber` - Source page in document
- `Analysis` - Optional analysis text

## Database Context

**Location:** `src/Infrastructure/Persistence/ContractIntelDbContext.cs`

The `ContractIntelDbContext` configures all entities with:

### Indexes for Performance
- **Contracts:**
  - `IX_Contracts_RenewalDate` - For renewal tracking queries
  - `IX_Contracts_RiskScore` - For risk-based filtering
  - `IX_Contracts_Vendor` - For vendor lookups

- **Documents:**
  - `IX_Documents_ContractId` - For contract document queries

- **Clauses:**
  - `IX_Clauses_ContractId_ClauseType` - Composite index for filtered clause queries

### Entity Configurations
- Required fields with max lengths
- Precision settings for decimal fields (`RiskScore`, `Confidence`)
- Cascade delete for related entities
- Proper foreign key relationships

## Dependency Injection Setup

**Location:** `src/Infrastructure/DependencyInjection.cs`

### Provider Switch
The system supports multiple database providers via configuration:

```json
{
  "Database": {
    "Provider": "Sqlite",  // Options: Sqlite, SqlServer, PostgreSQL
    "EnableSensitiveDataLogging": false
  },
  "ConnectionStrings": {
    "Sqlite": "Data Source=contractintel.db"
  }
}
```

### Usage in Program.cs
```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

### Adding New Providers
To add support for SQL Server or PostgreSQL:
1. Install the appropriate NuGet package:
   - SQL Server: `Microsoft.EntityFrameworkCore.SqlServer`
   - PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL`
2. Update connection string in appsettings.json
3. Change the `Database:Provider` setting

## EF Core Migrations

**Location:** `src/Infrastructure/Migrations/`

The project uses EF Core Migrations to manage database schema changes. Migrations are version-controlled and allow for safe schema evolution.

### Common Migration Commands

All commands should be run from the repository root directory.

**Add a new migration:**
```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebApi
```

**Update the database (apply pending migrations):**
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

**List all migrations:**
```bash
dotnet ef migrations list --project src/Infrastructure --startup-project src/WebApi
```

**Remove the last migration (if not yet applied):**
```bash
dotnet ef migrations remove --project src/Infrastructure --startup-project src/WebApi
```

**Generate SQL script for migrations:**
```bash
dotnet ef migrations script --project src/Infrastructure --startup-project src/WebApi
```

### Development Bootstrap

**Location:** `src/WebApi/Program.cs`

In development mode, migrations are automatically applied at startup:

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ContractIntelDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

**Note:** In production, apply migrations as part of your deployment process rather than at startup.

## Database Configuration

### Development (appsettings.Development.json)
- SQLite with sensitive data logging enabled
- EF Core query logging at Information level
- Database: `contractintel.db` in project root

### Production (appsettings.json)
- SQLite by default (can be changed)
- Sensitive data logging disabled
- Standard logging levels

## API Endpoints

A sample `ContractsController` is provided at `src/WebApi/Controllers/ContractsController.cs`:

- `GET /api/contracts` - List all contracts with related data
- `GET /api/contracts/{id}` - Get a specific contract
- `POST /api/contracts` - Create a new contract

## Testing the Setup

1. Build the solution:
   ```bash
   dotnet build
   ```

2. Run the API:
   ```bash
   dotnet run --project src/WebApi
   ```

3. Create a test contract:
   ```bash
   curl -X POST http://localhost:5000/api/contracts \
     -H "Content-Type: application/json" \
     -d '{
       "title": "Test Contract",
       "vendor": "Acme Corp",
       "startDate": "2024-01-01",
       "endDate": "2025-01-01",
       "renewalDate": "2024-12-01",
       "riskScore": 25.5,
       "status": "Active"
     }'
   ```

4. Verify the database was created:
   - Check for `contractintel.db` in the project root
   - Use a SQLite browser to inspect tables and indexes

## Next Steps

This foundation is ready for:
- Implementing repositories/unit of work pattern in Application layer
- Adding query specifications for complex business logic
- Implementing CQRS with MediatR
- Adding audit logging and soft deletes
- Implementing change tracking and optimistic concurrency

## Architecture

```
┌─────────────────┐
│   WebApi        │ - HTTP endpoints, DI configuration
└────────┬────────┘
         │
┌────────▼────────┐
│  Application    │ - Business logic (future)
└────────┬────────┘
         │
┌────────▼────────┐
│ Infrastructure  │ - DbContext, EF Core configuration
└────────┬────────┘
         │
┌────────▼────────┐
│    Domain       │ - Entities, value objects
└─────────────────┘
```

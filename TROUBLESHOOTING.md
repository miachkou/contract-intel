# Troubleshooting Guide

Common issues and quick fixes for Contract Intelligence development.

## EF Core Issues

### Version Mismatch Errors

**Symptoms:**
- `dotnet restore` fails with version conflicts
- `dotnet ef` commands fail with "No database provider has been configured"
- Design-time build errors

**Solution:**

1. **Check your EF Core tools version:**
   ```bash
   dotnet ef --version
   ```
   Should match 9.0.0 (same as the project's EF Core packages).

2. **Update EF Core tools:**
   ```bash
   dotnet tool update --global dotnet-ef --version 9.0.0
   ```

3. **Verify all EF packages are aligned:**
   ```bash
   grep "Microsoft.EntityFrameworkCore" src/**/*.csproj
   ```
   All should be version 9.0.0.

4. **Clean and restore:**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

### Design-Time Tooling Issues

**Symptom:** `dotnet ef migrations add` fails with "Unable to create an object of type 'ContractIntelDbContext'"

**Solution:**

Always specify both `--project` and `--startup-project`:
```bash
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/WebApi
```

The startup project (WebApi) contains the configuration, while the Infrastructure project contains the DbContext.

## NuGet Feed Issues

### Private Feed Interference

**Symptom:** `dotnet restore` fails or uses wrong package versions due to corporate/private NuGet feeds.

**Solution:**

The project includes a `NuGet.config` that enforces public-only packages:
```xml
<packageSources>
  <clear />
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
</packageSources>
```

If you still have issues:

1. **Clear NuGet cache:**
   ```bash
   dotnet nuget locals all --clear
   ```

2. **Verify NuGet sources:**
   ```bash
   dotnet nuget list source
   ```
   Should only show nuget.org.

3. **Check for global NuGet.config interference:**
   - Windows: `%APPDATA%\NuGet\NuGet.config`
   - macOS/Linux: `~/.nuget/NuGet/NuGet.config`
   
   Remove or comment out any private feeds in global config.

4. **Restore with explicit config:**
   ```bash
   dotnet restore --configfile NuGet.config
   ```

## SQLite + EF Core Issues

### Database Locked Errors

**Symptom:** "Database is locked" or "SQLite Error 5: database is locked"

**Common Causes:**
- Multiple connections accessing the database
- Long-running transactions
- Open SQLite database browser tools

**Solutions:**

1. **Close all database browser tools** (DB Browser for SQLite, etc.)

2. **Stop all running instances** of the application:
   ```bash
   # Find and kill processes
   ps aux | grep "WebApi"
   kill <PID>
   
   # Or on Windows
   tasklist | findstr "WebApi"
   taskkill /F /PID <PID>
   ```

3. **Enable busy timeout** (already configured in the project):
   The project uses `Timeout=30` in the connection string to wait up to 30 seconds.

4. **Delete and recreate the database** (development only):
   ```bash
   rm contractintel.db contractintel.db-shm contractintel.db-wal
   dotnet run --project src/WebApi
   ```
   Migrations will automatically run on startup in development mode.

### WAL Mode Issues

**Symptom:** Multiple `-wal` and `-shm` files in the database directory.

**Explanation:** SQLite uses Write-Ahead Logging (WAL) mode for better concurrency. The `.db-wal` and `.db-shm` files are temporary and managed automatically by SQLite.

**Solution:**

This is normal behavior. Do NOT delete these files while the application is running.

To checkpoint and consolidate:
```bash
# Stop the application first
sqlite3 contractintel.db "PRAGMA wal_checkpoint(TRUNCATE);"
```

### Migration Application Failures

**Symptom:** Migrations fail to apply during startup or manually.

**Solutions:**

1. **Check database file permissions:**
   ```bash
   ls -la contractintel.db
   ```
   Ensure the file is writable.

2. **Manually apply migrations:**
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
   ```

3. **Reset database** (development only):
   ```bash
   rm contractintel.db*
   dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
   ```

4. **View pending migrations:**
   ```bash
   dotnet ef migrations list --project src/Infrastructure --startup-project src/WebApi
   ```

## General Issues

### Build Failures

**Quick fix checklist:**
```bash
# 1. Clean everything
dotnet clean
rm -rf */obj */bin

# 2. Restore dependencies
dotnet restore

# 3. Build
dotnet build
```

### Port Already in Use

**Symptom:** Application fails to start with "Address already in use" error.

**Solution:**

1. **Find the process using the port:**
   ```bash
   # Backend (port 5058)
   lsof -i :5058
   
   # Frontend (port 5173)
   lsof -i :5173
   
   # On Windows
   netstat -ano | findstr :5058
   ```

2. **Kill the process:**
   ```bash
   kill <PID>
   
   # Or on Windows
   taskkill /F /PID <PID>
   ```

## Getting Help

If these solutions don't resolve your issue:

1. Check [README.md](README.md) for setup instructions
2. Review [PERSISTENCE.md](PERSISTENCE.md) for database details
3. Open an issue with:
   - Your environment (.NET version, OS, etc.)
   - Error message (full stack trace)
   - Steps to reproduce

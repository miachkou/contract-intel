# Contract Intel

Contract Intelligence API for contract analysis, clause detection, and risk scoring.

## Development Setup

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- VS Code (recommended)

### Getting Started

1. **Restore dependencies:**
   ```bash
   dotnet restore
   cd webapp && npm install
   ```

2. **Run the application:**

   Using VS Code tasks (recommended):
   - Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac) and select "Tasks: Run Task"
   - Choose:
     - **Run Backend (Watch)** - Runs the .NET API in watch mode with hot reload
     - **Run Frontend (Dev Server)** - Runs the Vite dev server
     - **Run Backend + Frontend** - Runs both concurrently

   Or manually:
   ```bash
   # Backend
   dotnet watch run --project src/WebApi/WebApi.csproj
   
   # Frontend
   cd webapp && npm run dev
   ```

3. **Access the application:**
   - API: http://localhost:5058
   - Swagger: http://localhost:5058/swagger
   - Frontend: http://localhost:5173

### Debugging

Use VS Code's debug configurations:

- **Debug Backend API** - Launch the .NET API with debugger attached
- **Attach to Backend API** - Attach to a running API process

Press `F5` or use the Run and Debug panel to start debugging.

### Building

```bash
# Backend
dotnet build

# Frontend
cd webapp && npm run build
```

### Testing

```bash
dotnet test
```

## Architecture

- **src/Domain** - Core domain entities and business logic
- **src/Application** - Application services and use cases
- **src/Infrastructure** - Data access, external services
- **src/WebApi** - REST API endpoints
- **webapp** - React frontend with Vite

## Database

The application uses SQLite by default for development. Migrations are applied automatically on startup in development mode.

To manually manage migrations:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

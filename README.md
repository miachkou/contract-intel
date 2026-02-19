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

### Testing with Sample PDFs

To test the contract extraction functionality:

1. **Create a test PDF** - See [docs/CREATE_TEST_PDF.md](docs/CREATE_TEST_PDF.md) for detailed instructions
2. **Use the provided sample** - A sample contract text file is available in `docs/sample_contract_text.txt`
3. **Understand PDF requirements** - Review [docs/PDF_REQUIREMENTS.md](docs/PDF_REQUIREMENTS.md) for supported formats

**Quick Start:**
- Open `docs/sample_contract_text.txt`
- Copy the content to Google Docs
- Download as PDF
- Upload to the application and run extraction

The sample contract includes all detectable clause types: renewal, auto-renewal, termination, data protection, liability caps, and governing law provisions.

## Architecture

- **src/Domain** - Core domain entities and business logic
- **src/Application** - Application services and use cases
- **src/Infrastructure** - Data access, external services
- **src/WebApi** - REST API endpoints
- **webapp** - React frontend with Vite

## Database

The application uses SQLite by default for development. On startup in development mode, the application automatically:
- Creates the database file if it doesn't exist
- Applies any pending migrations
- Seeds initial test data

To manually manage migrations (e.g., creating new migrations, rolling back, or running migrations in non-development environments):
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

See [docs/PERSISTENCE.md](docs/PERSISTENCE.md) for more details.

## Documentation

- [docs/CREATE_TEST_PDF.md](docs/CREATE_TEST_PDF.md) - Guide to creating test PDFs for the application
- [docs/PDF_REQUIREMENTS.md](docs/PDF_REQUIREMENTS.md) - PDF format requirements and limitations
- [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) - Common issues and solutions
- [docs/PERSISTENCE.md](docs/PERSISTENCE.md) - Database architecture and management
- [docs/PR_REVIEW_GUIDE.md](docs/PR_REVIEW_GUIDE.md) - Code review guidelines
- [docs/AI_PROMPTS.md](docs/AI_PROMPTS.md) - Development history and AI-assisted prompts

## Contributing

### PR Review Helper

We provide a PR review helper to streamline the review process. See [docs/PR_REVIEW_GUIDE.md](docs/PR_REVIEW_GUIDE.md) for comprehensive review guidelines.

To use the automated helper:

```bash
cd scripts
npm install
npm run pr-helper generate  # Generate PR description and commit message
npm run pr-helper review    # Quick review checklist
```

The helper analyzes your changes and provides:
- Conventional commit message suggestions
- PR description template with your changes categorized
- Code analysis for common issues (async patterns, error handling, performance, etc.)
- Review checklist

See [scripts/README.md](scripts/README.md) for more details.

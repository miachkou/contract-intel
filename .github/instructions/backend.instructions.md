# Backend Instructions (Path-specific)

This file provides path-specific guidance for AI assistants working on backend code under `src/`.

## Backend Guidance
- **Follow clean architecture layers**: Keep business logic in `Application`, domain rules in `Domain`, data access in `Infrastructure`, and API contracts in `WebApi`.
- **Use DI & interfaces**: Prefer injecting services via interfaces from `Application/Interfaces` and register implementations in `Infrastructure/DependencyInjection.cs` or `WebApi/Program.cs`.
- **Keep controllers thin**: Controllers should delegate to services and return simple DTOs.
- **Prefer `async/await`**: Avoid `.Result`/`.Wait()` and always accept `CancellationToken` in public async methods.
- **Tests**: Add unit tests under `tests/ContractIntel.Tests` for new business logic; use xUnit and `NullLogger` when needed.

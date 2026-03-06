# Tests Instructions (Path-specific)

This file provides guidance for AI assistants working on automated tests in `tests/`.

## Tests Guidance
- **Unit tests first**: Prefer unit tests that validate business logic in `src/Application` and `src/Domain` layers.
- **Use xUnit**: The backend tests use xUnit; follow existing patterns in `tests/ContractIntel.Tests`.
- **Arrange/Act/Assert**: Keep tests structured and readable. Use small helper methods where it improves clarity.
- **Mocks & fakes**: Use lightweight fakes/mocks (e.g., `NullLogger`) rather than full DI containers unless needed.
- **Fast & deterministic**: Avoid integration-style tests that depend on external services or timing.

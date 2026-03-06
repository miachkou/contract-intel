# Copilot Instructions (Repo-wide)

These instructions provide always-on context for GitHub Copilot and other AI assistants operating within this repository.

## Purpose
This file defines the default behavior and expectations for AI-generated suggestions, PR descriptions, and other assisted workflows.

## Always‑On Context
- **Be concise and factual.** Provide clear, actionable guidance and avoid unnecessary verbosity.
- **Follow repository conventions.** Respect the existing architecture (Clean Architecture with `Domain`, `Application`, `Infrastructure`, `WebApi`) and folder layout.
- **Respect naming and style.** Use the same naming patterns, idioms, and coding styles already present in the project.
- **Avoid breaking changes.** Prefer safe, incremental changes; only introduce breaking changes when explicitly requested.
- **Focus on correctness & maintainability.** Prioritize readable, testable code and follow the existing patterns for dependency injection, layering, and error handling.
- **Ask clarifying questions when uncertain.** If a request is ambiguous or missing critical information, ask for clarification rather than guessing.

## Scope
- Applies to AI interactions across the entire repository (backend, frontend, docs, scripts).
- Path-specific instructions should be placed under `.github/instructions/` if needed.

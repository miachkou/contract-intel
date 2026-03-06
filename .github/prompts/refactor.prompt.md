# Refactor Prompt Template

Use this prompt when asking the assistant to refactor existing code for readability, maintainability, or testability without changing behavior.

## Variables

- `{{targetArea}}` — The area of code to refactor (e.g., "contract extraction pipeline", "API controllers").
- `{{reason}}` — Why this refactor is needed (e.g., "reduce duplication", "improve testability").
- `{{constraints}}` — Constraints (e.g., "no behavior changes", "keep public API stable").
- `{{notes}}` — Additional context or requirements.

---

### Refactor Task

Refactor the following area of the codebase:

**Target Area:** {{targetArea}}

**Reason:** {{reason}}

### Goals
- Improve code readability and maintainability.
- Keep behavior unchanged.
- Add or improve unit tests where appropriate.

### Constraints / Notes
{{constraints}}

{{notes}}

# Bug Fix Prompt Template

Use this prompt when asking the assistant to identify, diagnose, and fix a bug in the codebase.

## Variables

- `{{symptom}}` — A short description of the observed problem (e.g., "API returns 500 when retrieving contracts").
- `{{reproductionSteps}}` — Steps to reproduce the issue.
- `{{filePaths}}` — The relevant file(s) or module(s) to inspect.
- `{{constraints}}` — Any constraints (e.g., must preserve API contracts, no breaking changes).
- `{{notes}}` — Additional context (e.g., recent refactors, related issues).

---

### Bug Fix Task

We are experiencing the following issue:

**Symptom:** {{symptom}}

**Reproduction Steps:**
{{reproductionSteps}}

**Relevant Files/Areas:**
{{filePaths}}

### Goals
- Identify the root cause of the issue.
- Provide a minimal and correct fix.
- Add or update tests to prevent regressions.

### Constraints / Notes
{{constraints}}

{{notes}}

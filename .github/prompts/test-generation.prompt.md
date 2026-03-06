# Test Generation Prompt Template

Use this prompt when asking the assistant to add or improve automated tests for a specific piece of functionality.

## Variables

- `{{subject}}` — The class, method, or feature under test.
- `{{testScope}}` — What should be tested (e.g., edge cases, error handling, happy path).
- `{{framework}}` — Test framework / style (e.g., xUnit, React Testing Library).
- `{{constraints}}` — Any special constraints (e.g., keep test runtime fast, avoid flakiness).
- `{{notes}}` — Additional context.

---

### Test Generation Task

Generate or improve tests for:

**Subject:** {{subject}}

**Scope:** {{testScope}}

### Goals
- Ensure behavior is correctly asserted.
- Cover key edge cases and error conditions.
- Keep tests fast, deterministic, and easy to understand.

### Constraints / Notes
{{constraints}}

{{notes}}

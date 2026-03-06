# Documentation Update Prompt Template

Use this prompt when asking the assistant to update or create documentation (README, guides, architecture docs, etc.).

## Variables

- `{{docTarget}}` — The document or section to update (e.g., "README installation section", "architecture overview").
- `{{changes}}` — What needs to be added or changed.
- `{{audience}}` — Who the docs are for (e.g., "new contributors", "API consumers").
- `{{constraints}}` — Any constraints (e.g., keep language simple, keep short).
- `{{notes}}` — Additional context.

---

### Documentation Task

Update or create documentation for:

**Target:** {{docTarget}}

**Changes:**
{{changes}}

### Goals
- Make the documentation clear, accurate, and easy to follow.
- Ensure it aligns with existing style and structure.
- Include examples or commands when relevant.

### Constraints / Notes
{{constraints}}

{{notes}}

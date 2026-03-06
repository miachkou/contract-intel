# Copilot Usage Guide

This guide explains how to use GitHub Copilot and Copilot Chat effectively in the Contract Intel repository.

## 1) Where to find guidance

### Repo-wide instructions
- **File:** `.github/copilot-instructions.md`
- **Purpose:** Always‑on behavior rules and high-level expectations for AI-generated code and prompts.

### Path-specific instructions
- **Folder:** `.github/instructions/`
- **Purpose:** Tailored guidance for different parts of the project (backend, frontend, docs, tests, scripts).
- **Examples:**
  - `backend.instructions.md` — backend architecture, DI, controller patterns
  - `frontend.instructions.md` — React/TypeScript patterns, API client usage
  - `docs.instructions.md` — doc style and structure
  - `tests.instructions.md` — testing style and conventions
  - `scripts.instructions.md` — scripting best practices

## 2) Using prompt templates

### Where they live
- **Folder:** `.github/prompts/`
- **Purpose:** Provide consistent structured prompts for common tasks.

### Common templates
- `ai-dev-step.prompt.md` — general feature/task prompt
- `bug-fix.prompt.md` — diagnosing/fixing bugs
- `refactor.prompt.md` — refactoring without changing behavior
- `test-generation.prompt.md` — creating or improving tests
- `docs-update.prompt.md` — updating or adding documentation

### How to use
1. Open the relevant `.prompt.md` file.
2. Replace the `{{...}}` placeholders with real values.
3. Copy the filled prompt into Copilot Chat or your assistant interface.

## 3) Copilot configuration

### File
- `.github/copilot.yml`

### What it controls
- Enables inline suggestions and Copilot Chat.
- Excludes certain paths (docs, scripts, node_modules) from suggestions to reduce noise.

## 4) Best practices when using Copilot

- **Validate changes**: Always review AI-generated code the same way you would any PR.
- **Keep changes small**: Use Copilot for focused tasks (e.g., a single bug fix, one refactor) rather than broad rewrites.
- **Ask clarifying questions**: If the assistant output is unclear, provide more context and iteratively refine the prompt.
- **Document AI-aided work**: When a PR includes generated code, note it in the PR description (e.g., "Generated with Copilot using `<prompt>`").

## 5) When not to rely on Copilot

- Complex architectural decisions that require deep domain knowledge
- Security-sensitive code (e.g., authentication, encryption) without thorough review
- Anything where correctness is critical and must be verified by an expert

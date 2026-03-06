# AI Assistant Prompt Template (Step-based)

This file is a reusable prompt template for generating or iterating on development tasks. It is designed to be filled in with the variables below and used as the system/user prompt for an AI assistant.

---

## Variables

- `{{projectName}}` — The name of the project (e.g., "Contract Intelligence").
- `{{branchName}}` — (Optional) The git branch name to work on (e.g., `feat/core-data-model`).
- `{{taskTitle}}` — A short title for the task (e.g., "Core Data Model & Persistence").
- `{{taskDescription}}` — A brief description of what needs to be implemented.
- `{{goals}}` — A bullet list of clear goals or acceptance criteria.
- `{{constraints}}` — Any important constraints, preferences, or additional guidance.
- `{{notes}}` — Any extra context or clarifications that may help.

---

## Prompt Template

You are an expert software engineer and AI assistant helping to implement a feature for **{{projectName}}**.

{{#if branchName}}
**Branch:** `{{branchName}}`
{{/if}}

### Task
{{taskTitle}}

{{taskDescription}}

### Goals
{{goals}}

### Constraints / Notes
{{constraints}}

{{notes}}

---

> **Usage:** Replace the `{{...}}` placeholders with actual values. Keep prompts concise, focused, and aligned with the project’s clean architecture and code quality standards.

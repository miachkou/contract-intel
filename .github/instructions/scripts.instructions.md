# Scripts Instructions (Path-specific)

This file provides guidance for AI assistants working on project scripts (e.g., `scripts/`).

## Scripts Guidance
- **Keep scripts simple**: Scripts should solve a narrow task (e.g., generating PR descriptions, running checks).
- **Cross-platform**: Prefer Node.js (JS/TS) scripts or cross-platform shell commands to support Windows/macOS/Linux.
- **Documentation**: Update README or script help output when adding new scripts.
- **Avoid secrets**: Do not store credentials or secrets in scripts; use environment variables when needed.

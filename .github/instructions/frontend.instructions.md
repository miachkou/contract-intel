# Frontend Instructions (Path-specific)

This file provides guidance for AI assistants working on frontend code under `webapp/`.

## Frontend Guidance
- **Use React best practices**: Prefer functional components with hooks, avoid unnecessary rerenders, and keep components focused.
- **State management**: Use React Query for server state; use local component state only when appropriate.
- **API access**: Use the centralized `webapp/src/lib/api.ts` client rather than calling `fetch` directly.
- **Type safety**: Prefer explicit TypeScript types; avoid `any` and keep type definitions close to usage.
- **Styling**: Keep styles scoped (CSS modules / component-level) and avoid global style side effects.
- **Testing**: Add/extend React Testing Library tests under `webapp/src/__tests__` where appropriate; keep tests fast and deterministic.

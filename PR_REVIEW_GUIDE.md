# PR Review Guide

This guide provides a structured approach to reviewing pull requests in the Contract Intel project.

## Quick Review Checklist

### 1. Code Quality & Best Practices

#### Null Safety & Error Handling
- [ ] **Null checks**: Are nullable references properly checked?
  - C#: Use null-coalescing (`??`), null-conditional (`?.`), or explicit null checks
  - TypeScript: Check for `null` and `undefined` where appropriate
- [ ] **Error handling**: Are exceptions caught and handled appropriately?
  - C#: Use try-catch blocks for operations that can fail (file I/O, external APIs, database)
  - TypeScript: Handle promise rejections and API errors
- [ ] **Async patterns**: Are async operations handled correctly?
  - C#: Use `async`/`await` properly, don't mix `.Result` or `.Wait()`
  - TypeScript: Use `async`/`await` consistently, avoid callback hell

#### Query & Performance
- [ ] **Database queries**: Are queries efficient?
  - Use proper indexing
  - Avoid N+1 queries
  - Consider using `.AsNoTracking()` for read-only queries in EF Core
- [ ] **API efficiency**: Are multiple API calls batched when possible?
- [ ] **Memory usage**: Are large collections or streams handled efficiently?

### 2. Code Smells & Readability

- [ ] **Long methods**: Are methods under 50 lines? Consider breaking down complex logic
- [ ] **Magic numbers**: Are magic numbers replaced with named constants?
- [ ] **Duplicate code**: Is there repeated code that should be extracted?
- [ ] **Naming**: Are variables, methods, and classes named clearly?
- [ ] **Comments**: Are complex algorithms explained? (but prefer self-documenting code)
- [ ] **Dead code**: Is there commented-out code or unused imports that should be removed?

### 3. Architecture & Patterns

#### Backend (C# / .NET)
- [ ] **Separation of concerns**: 
  - Domain logic in `Domain` layer
  - Business logic in `Application` layer
  - Data access in `Infrastructure` layer
  - API concerns in `WebApi` layer
- [ ] **Dependency injection**: Are services registered in appropriate DI configuration?
- [ ] **Interface usage**: Are dependencies injected via interfaces from `Application.Interfaces`?
- [ ] **Repository pattern**: Is data access going through repositories?

#### Frontend (TypeScript / React)
- [ ] **Component structure**: Are components focused and reusable?
- [ ] **State management**: Is state lifted appropriately? Consider React Query for server state
- [ ] **API calls**: Use the `api.ts` client, not direct fetch calls
- [ ] **Type safety**: Are types properly defined and used?
- [ ] **CSS scoping**: Are styles scoped to avoid global conflicts?

### 4. Testing
- [ ] **Test coverage**: Are new features covered by tests?
- [ ] **Test quality**: Do tests actually validate the intended behavior?
- [ ] **Test naming**: Are test names descriptive (e.g., `Should_ReturnError_When_InputIsNull`)?
- [ ] **xUnit conventions**: Backend tests use xUnit with NullLogger for services requiring ILogger

### 5. Security
- [ ] **Input validation**: Are user inputs validated?
- [ ] **SQL injection**: Are parameterized queries used?
- [ ] **XSS prevention**: Is user content properly escaped?
- [ ] **Authentication/Authorization**: Are endpoints properly protected?
- [ ] **Sensitive data**: Are secrets kept out of source code?

### 6. Documentation
- [ ] **XML docs**: Are public APIs documented with XML comments (C#)?
- [ ] **README updates**: Are setup/usage instructions updated if needed?
- [ ] **API changes**: Are breaking API changes noted?

## PR Description Template

Use this template when creating or reviewing PRs:

```markdown
## Summary
<!-- Brief description of what this PR does (2-3 sentences) -->

## Changes
<!-- List of specific changes made -->
- 
- 
- 

## Acceptance Criteria
<!-- What should work after this PR is merged? -->
- [ ] 
- [ ] 
- [ ] 

## Manual Testing Steps
<!-- How to verify these changes work -->
1. 
2. 
3. 

## Related Issues
<!-- Link to related issues -->
Closes #

## Notes
<!-- Any additional context, decisions, or trade-offs -->
```

## Conventional Commit Messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

### Format
```
<type>(<scope>): <short description>

<optional body>

<optional footer>
```

### Types
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, missing semi-colons, etc.)
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Changes to build process, dependencies, or auxiliary tools

### Scopes (examples)
- `api`: WebApi layer
- `domain`: Domain layer
- `infra`: Infrastructure layer
- `frontend`: React frontend
- `db`: Database/migrations

### Examples
```
feat(api): add contract renewal reminder endpoint
fix(frontend): prevent duplicate file uploads
docs(readme): add PR review guidelines
refactor(domain): extract risk calculation to separate service
test(api): add integration tests for contracts controller
chore(deps): update .NET SDK to 9.0.1
```

## Review Process

### For Authors
1. **Before creating PR**:
   - Run `dotnet test` to ensure tests pass
   - Run `dotnet build` to check for compilation errors
   - Run `cd webapp && npm run build` to check frontend builds
   - Review your own changes first

2. **Creating the PR**:
   - Use the PR description template above
   - Add appropriate labels
   - Request reviewers
   - Link related issues

3. **After review**:
   - Address all comments
   - Mark resolved comments
   - Request re-review if significant changes made

### For Reviewers
1. **First pass**: High-level review
   - Does it solve the stated problem?
   - Is the approach reasonable?
   - Are there architectural concerns?

2. **Detailed review**: Use checklist above
   - Code quality
   - Security
   - Testing
   - Documentation

3. **Provide feedback**:
   - Be constructive and specific
   - Distinguish between "must fix" and "nice to have"
   - Suggest alternatives when possible
   - Approve when ready

## Common Pitfalls

### Backend
- ❌ Using `.Result` or `.Wait()` on Tasks (blocks threads)
- ❌ Not disposing of IDisposable objects (use `using` statements)
- ❌ Catching generic `Exception` without rethrowing
- ❌ Not using CancellationToken for async operations
- ❌ Mixing sync and async code

### Frontend
- ❌ Not handling loading and error states
- ❌ Mutating state directly instead of using setState
- ❌ Forgetting to clean up effects (return cleanup function)
- ❌ Making API calls in components instead of using React Query
- ❌ Not memoizing expensive computations

## Resources

- [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [React Best Practices](https://react.dev/learn/thinking-in-react)
- [TypeScript Best Practices](https://www.typescriptlang.org/docs/handbook/declaration-files/do-s-and-don-ts.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

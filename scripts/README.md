# PR Review Helper Scripts

This directory contains helper scripts for reviewing and creating pull requests.

## Installation

```bash
cd scripts
npm install
```

## Usage

### Generate PR Description

Generate a PR description and conventional commit message based on your changes:

```bash
npm run pr-helper generate
```

This will:
- Analyze changed files in your branch
- Detect common code issues
- Generate a PR description template
- Suggest a conventional commit message

### Quick Review

Get a quick review checklist for your changes:

```bash
npm run pr-helper review
```

This will:
- Show file change statistics
- Highlight potential issues
- Provide a manual review checklist

## Options

Both commands support comparing against a different base branch:

```bash
npm run pr-helper generate -- --base origin/develop
npm run pr-helper review -- --base origin/develop
```

## What It Checks

The helper analyzes code for:

### Backend (C#)
- Async/await patterns (avoiding `.Result` and `.Wait()`)
- Null safety
- Error handling
- IDisposable usage
- N+1 query patterns
- Magic numbers

### Frontend (TypeScript/React)
- Error handling in async operations
- Direct fetch calls (should use api client)
- setState in loops
- useEffect cleanup
- TypeScript `any` usage
- console.log statements

## See Also

- [PR_REVIEW_GUIDE.md](../docs/PR_REVIEW_GUIDE.md) - Comprehensive review guidelines

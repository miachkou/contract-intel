# AI Development Prompts

This document contains the prompts used during the development of the Contract Intelligence application.

## Project Context

- **Project Name:** Contract Intelligence MVP
- **AI Assistant:** GitHub Copilot
- **Development Period:** January - February 2026
- **Status:** Completed

### Project Description
A contract management system with AI-powered clause detection, risk scoring, and renewal tracking capabilities. The application analyzes PDF contracts, extracts key clauses, calculates risk scores, and monitors upcoming renewals.

### Tech Stack
- **Backend:** .NET 9, ASP.NET Core, Entity Framework Core, SQLite/PostgreSQL
- **Frontend:** React, TypeScript, Vite, React Query, Axios
- **AI/ML:** Rule-based clause detection with regex patterns
- **Infrastructure:** GitHub Actions, VS Code tasks

## Overview

This file serves as a record of the AI-assisted development process, documenting the key prompts and interactions that shaped the application's architecture and implementation. The project was built iteratively over 20 steps, following clean architecture principles.

---

## Prompts

## Step 1 — Core Data Model & Persistence

Step 1 — Core Data Model & Persistence

Before starting, create a new branch:
```
git checkout -b feat/core-data-model
```
Please create the core data model for our Contract Clause & Renewal Intelligence MVP.

Goals:
- Define simple, clean entities for contracts, documents, and clauses.
- Add a minimal DbContext with sensible indexes.
- Use SQLite by default, but allow easy switching of the provider.
- Include a small development-only bootstrap that ensures the database exists.
- Keep the design minimal, idiomatic, and aligned with clean architecture.

Let Copilot decide naming, folders, and structure.

---

## Step 2 — Repositories & Querying

Step 2 — Repositories & Querying

Before starting, create a new branch:
```
git checkout -b feat/repositories
```
Please add a simple repository layer to work with contracts, documents, and clauses.

Goals:
- Introduce a small generic repository abstraction.
- Add a contract-focused repository with:
  - load by id
  - search with filtering (vendor contains, min risk, renewal before)
  - list upcoming renewals
- Use no-tracking for read queries.
- Keep everything minimal, clean, and easy to test.
- Register repositories in DI.

Let Copilot choose naming, structure, and finer details.

---

## Step 3 — Local File Storage

Step 3 — Local File Storage

Before starting, create a new branch:
```
git checkout -b feat/local-file-storage
```
Please add a simple local file storage service for the backend.

Goals:
- Create a small interface for saving, opening, checking, and deleting files.
- Implement a local filesystem storage that saves files in a clean folder structure.
- Sanitize filenames and ensure directories are created as needed.
- Return a relative path that can be saved in the database.
- Handle errors gracefully.
- Register everything in DI.

Keep it simple and let Copilot decide naming and structure.

---

## Step 4 — PDF Text Extraction

Step 4 — PDF Text Extraction

Before starting, create a new branch:
```
git checkout -b feat/pdf-text-extraction
```
Please add a text extraction service using PdfPig.

Goals:
- Define a small interface that extracts full text and per-page text from a PDF stream.
- Implement a PdfPig-based extractor that handles malformed or empty PDFs gracefully.
- Normalize line breaks minimally.
- No OCR, no extra formats.
- Register it cleanly in DI.

Copilot may pick names, DTOs, and layout.

---

## Step 5 — Rule-Based Clause Detector

Step 5 — Rule-Based Clause Detector

Before starting, create a new branch:
```
git checkout -b feat/clause-detector
```
Please implement a rule-based clause detector using regex.

Goals:
- Create a simple interface for detecting clauses in extracted text.
- Detect renewal, auto_renewal, termination, data_protection, liability_cap, and governing_law.
- Return clause type, excerpt, confidence, and (optional) page info.
- Keep regex patterns readable and testable.
- Register the detector in DI.

Let Copilot choose naming and structure.

---

## Step 6 — Risk Scoring Service

Step 6 — Risk Scoring Service

Before starting, create a new branch:
```
git checkout -b feat/risk-scoring
```
Please add a small risk scoring service.

Goals:
- Compute a normalized risk score based on detected clauses.
- Penalize missing required clauses.
- Add extra penalty for auto-renewal with short notice.
- Make weighting configurable via appsettings.
- Keep implementation clean and deterministic.
- Register the service in DI.

Copilot can choose naming and layout.

---

## Step 7 — Inline Extraction Service

Step 7 — Inline Extraction Service

Before starting, create a new branch:
```
git checkout -b feat/inline-extraction
```
Please implement an inline extraction service that runs the full pipeline.

Goals:
- Load the latest document for a contract.
- Read it using file storage.
- Extract text.
- Detect clauses.
- Replace stored clauses for the contract.
- Recompute risk and update the contract.
- Return a simple summary (counts + risk).
- Handle common error cases cleanly.

Keep the design minimal and pragmatic.

---

## Step 8 — Minimal API Endpoints

Step 8 — Minimal API Endpoints

Before starting, create a new branch:
```
git checkout -b feat/minimal-api
```
Please expose minimal API endpoints for the MVP.

Endpoints:
- Create/list/get contracts with filters.
- Upload a document via multipart.
- Trigger inline extraction.
- List clauses for a contract.

Goals:
- Keep handlers simple.
- Validate inputs and return clear errors.
- Use DI for repos and services.
- Minimal logging.

Let Copilot decide models and structure.

---

## Step 9 — React Base App & Routing

Step 9 — React Base App & Routing

Before starting, create a new branch:
```
git checkout -b feat/frontend-base-routing
```
Please set up the basic React app with routing.

Goals:
- Add routes for /contracts, /contracts/:id, and /renewals.
- Add a simple header or nav bar.
- Create a shared Axios client.
- Add placeholder screens for each route.

Keep structure clean and lightweight.

---

## Step 10 — Contracts List with Filters

Step 10 — Contracts List with Filters

Before starting, create a new branch:
```
git checkout -b feat/frontend-contracts-list
```
Please implement the contracts list screen.

Goals:
- Show a table/list with vendor, renewal date, and risk score.
- Add filters for renewal window and min risk.
- Fetch data using React Query.
- Link each item to contract details.

Keep UI minimal and readable.

---

## Step 11 — Contract Details

Step 11 — Contract Details

Before starting, create a new branch:
```
git checkout -b feat/frontend-contract-details
```
Please build the contract details page.

Goals:
- Show contract overview.
- Upload PDFs and refresh state.
- Trigger extraction.
- Display detected clauses with excerpt and confidence.
- Refresh risk score after extraction.

Keep layout simple and clean.

---

## Step 12 — Renewals View

Step 12 — Renewals View

Before starting, create a new branch:
```
git checkout -b feat/frontend-renewals
```
Please implement a renewals screen.

Goals:
- Input for window in days.
- Optional minimum risk filter.
- List contracts renewing within the selected period.
- Link to contract details.

Keep UI minimal and functional.

---

## Step 13 — Edit / Approve Clause

Step 13 — Edit / Approve Clause

Before starting, create a new branch:
```
git checkout -b feat/clause-edit-approve
```
Please add a simple clause editing and approval flow.

Goals:
- Backend endpoint to update a clause (type/excerpt).
- Support approving a clause.
- Recompute risk after changes.
- Frontend UI for editing and approving.

Keep everything simple and readable.

---

## Step 14 — Dev Seed Data

Step 14 — Dev Seed Data

Before starting, create a new branch:
```
git checkout -b chore/dev-seed-data
```
Please add a small dev-only seed mechanism.

Goals:
- Insert a few sample contracts.
- Idempotent behavior.
- Trigger automatically in development or via a small dev endpoint.

Keep it lightweight.

---

## Step 15 — Basic Tests

Step 15 — Basic Tests

Before starting, create a new branch:
```
git checkout -b test/basic-unit-tests
```
Please add a lightweight test project.

Goals:
- Unit tests for clause detection regexes.
- Unit tests for risk scoring logic.
- Keep tests small, fast, and readable.

Let Copilot structure the project naturally.

---

## Step 16 — EF Migrations

Step 16 — EF Migrations

Before starting, create a new branch:
```
git checkout -b chore/ef-migrations
```
Please add EF Core migrations for the initial schema.

Goals:
- Enable migrations.
- Add the first migration.
- Apply migrations automatically in development.
- Document basic migration commands.

Keep implementation clean.

---

## Step 17 — CI Workflow

Step 17 — CI Workflow

Before starting, create a new branch:
```
git checkout -b chore/ci-workflow
```
Please add a simple GitHub Actions workflow.

Goals:
- Backend job (restore, build, test).
- Frontend job (install, build).
- Run jobs in parallel.
- Minimal configuration and clean YAML.

Keep the workflow lightweight.

---

## Step 18 — VS Code Developer Tasks

Step 18 — VS Code Developer Tasks

Before starting, create a new branch:
```
git checkout -b chore/vscode-tasks
```
Please add VS Code tasks and launch configurations.

Goals:
- Tasks to run backend and frontend in dev mode.
- Optional combined task.
- Debug config for the API.

Keep configs simple and cross-platform where possible.

---

## Step 19 — PR Review Helper

Step 19 — PR Review Helper

Before starting, create a new branch:
```
git checkout -b chore/pr-review-helper
```
Please add a small helper for reviewing PRs.

Goals:
- Provide suggested improvement categories.
- Generate a short conventional commit.
- Provide a PR description template.

Format can be markdown or similar.

---

## Step 20 — Troubleshooting Helpers

Step 20 — Troubleshooting Helpers

Before starting, create a new branch:
```
git checkout -b chore/troubleshooting-helpers
```
Please add a short troubleshooting doc.

Topics:
- EF version issues.
- NuGet feed problems.
- SQLite locked database issues.

Keep instructions short and copy‑paste friendly.

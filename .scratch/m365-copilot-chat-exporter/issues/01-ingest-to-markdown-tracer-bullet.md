# 01: Foundation & First Tracer Bullet: Ingest-to-Markdown Slice

Status: ready-for-agent

**Blocked by:** None (can start immediately)

## What to build
An end-to-end working slice of the application that initializes the .NET 8 / Avalonia solution structure across Desktop and Mobile targets, establishes core domain entities and local SQLite storage, ingests conversation turn fixtures, and formats them into a clean GitHub Flavored Markdown file with YAML frontmatter and footnote citations, verified through automated integration tests at the application service seam.

## Acceptance Criteria

- [x] **Solution Scaffolding**: Solution file `CopilotChatExporter.sln` builds cleanly with `dotnet build` and contains:
  - `src/CopilotChatExporter.Core` (.NET 8+ class library, zero UI dependencies)
  - `src/CopilotChatExporter.Infrastructure` (.NET 8+ class library)
  - `src/CopilotChatExporter.UI` (Avalonia shared XAML library)
  - `src/CopilotChatExporter.Desktop` (Avalonia desktop executable head)
  - `src/CopilotChatExporter.Mobile` (Avalonia mobile project)
  - `tests/CopilotChatExporter.Core.Tests` (xUnit test project)
- [x] **Domain Schema & Ingestion**:
  - Domain models `Conversation`, `Turn`, and `Citation` are defined adhering to ubiquitous language in `CONTEXT.md`.
  - SQLite schema initializes tables `conversations`, `turns`, and `citations`.
  - Ingesting a conversation with N turns and M citations persists them with exact relational integrity and timestamps.
- [x] **Markdown Export Generation**:
  - Output file contains YAML frontmatter (`id`, `title`, `created_at`, `updated_at`, `turns`).
  - Dialogue turns alternate cleanly between user prompts and Copilot responses.
  - Citations are represented inline as numbered footnote references (`[^1]`, `[^2]`) linked to a concluding `## Sources & Citations` reference list with clickable URLs and titles.
  - Sanitized filename conforms to `{YYYY-MM-DD} - {SanitizedTitle}.md`, stripping illegal OS path characters.
- [x] **Automated Seam Verification**:
  - Automated integration test at the `ICopilotExportService` seam passes with `dotnet test`, executing end-to-end from ingestion to markdown file generation against in-memory SQLite (`Filename=:memory:`).

## Verification
Run `dotnet test tests/CopilotChatExporter.Core.Tests` to verify domain ingestion and Markdown export formatting tests pass.

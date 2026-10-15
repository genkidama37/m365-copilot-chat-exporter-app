# Domain Models and SQLite Persistence

Status: ready-for-agent

## Description

Implement core domain entities (`Conversation`, `Turn`, `Citation`) and the local embedded SQLite repository in `CopilotChatExporter.Core`.

## Scope of Work

1. Define immutable domain models:
   - `Conversation` (ID, Title, CreatedAt, UpdatedAt, TurnCount, LastExportedAt, Turns)
   - `Turn` (ID, ConversationId, TurnIndex, Prompt, PromptTimestamp, Response, ResponseTimestamp, Citations, AssetUrls)
   - `Citation` (ID, Index, Title, Url, Snippet)
2. Implement `ICatalogRepository` using `Microsoft.Data.Sqlite`:
   - Database schema initialization (tables: `conversations`, `turns`, `citations`).
   - SQLite FTS5 virtual table (`turns_fts`) for full-text search across user prompts and Copilot responses.
   - CRUD operations for persisting synced conversations and turns.
   - Query methods for search by keyword, date range filtering, and pagination.
   - Tracking of `LastExportedAt` timestamps.
3. Write comprehensive unit tests in `CopilotChatExporter.Core.Tests` validating repository operations against in-memory SQLite (`Filename=:memory:`).

## Acceptance Criteria

- All domain entities adhere to terminology defined in `CONTEXT.md`.
- Schema creates cleanly and handles incremental sync upserts without data corruption.
- Full-text search and date range filtering queries pass automated unit tests.

## Comments

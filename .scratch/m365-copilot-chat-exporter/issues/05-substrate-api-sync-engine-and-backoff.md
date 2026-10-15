# 05: Live Substrate API Sync Engine & Rate-Limit Backoff Slice

Status: ready-for-agent

**Blocked by:** 01: Foundation & First Tracer Bullet: Ingest-to-Markdown Slice, 04: Embedded Webview Session Interceptor & Secure Keyring Slice

## What to build
An end-to-end background synchronization engine that connects authenticated sessions to Microsoft Substrate endpoints, paging conversation lists and retrieving turns into local SQLite with controlled concurrency, exponential backoff for HTTP 429 (`Retry-After`), and real-time sync progress reporting in the UI.

## Acceptance Criteria

- [ ] **Substrate API Client**:
  - Implements `GET /m365Copilot/chats?$top=50&$skipToken=...` to page through conversation summaries.
  - Implements `GET /m365Copilot/chats/{id}/messages` to retrieve all turns and citations for a conversation.
  - Maps API response payloads cleanly to domain entities.
- [ ] **Throttling & Concurrency Control**:
  - Limits concurrent network requests to a maximum of 2 parallel calls.
- [ ] **Rate-Limit Resilience (HTTP 429)**:
  - Detects HTTP 429 responses, parses the `Retry-After` header, and applies exponential backoff with jitter.
- [ ] **Reconciliation & Persistence**:
  - Compares remote conversation timestamps against local SQLite records to sync only new or updated items.
  - Persists synced turns transactionally into local SQLite without data corruption.
- [ ] **Progress & Cancellation**:
  - Emits real-time progress updates (`IProgress<SyncProgress>`) with current count, total count, and current phase.
  - Supports clean cancellation via `CancellationToken` leaving the database in a consistent state.
- [ ] **Automated Seam Verification**:
  - Integration tests with mock `HttpMessageHandler` verify multi-page pagination, rate-limit backoff timing, incremental sync reconciliation, and cancellation.

## Verification
Run `dotnet test --filter "FullyQualifiedName~SyncEngineTests"`.

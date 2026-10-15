# Substrate API Client and Sync Engine

Status: ready-for-agent

## Description

Implement the Substrate HTTP client and background synchronization engine in `CopilotChatExporter.Infrastructure` to fetch paginated conversations and turns with rate-limit resiliency.

## Scope of Work

1. Implement `ISubstrateApiClient`:
   - Fetch conversation catalog pages (`GET /m365Copilot/chats?$top=50&$skipToken=...`).
   - Fetch turns/messages for a conversation (`GET /m365Copilot/chats/{id}/messages`).
   - Parse JSON payloads into core domain structures.
2. Implement throttling and resilience:
   - Cap concurrent API requests (max 2 parallel).
   - Inspect HTTP 429 response status and parse `Retry-After` header.
   - Implement exponential backoff with jitter.
3. Implement `ISyncEngine`:
   - Orchestrate incremental sync: compare remote list with local SQLite records by `updated_at`.
   - Report live progress (`Progress<SyncProgressStatus>`) emitting current conversation index, total conversations, and phase (fetching headers vs fetching turns).
   - Support graceful cancellation via `CancellationToken`.
4. Unit and integration tests using mocked `HttpMessageHandler`.

## Acceptance Criteria

- Handles pagination across multi-page conversation histories.
- Successfully recovers from simulated HTTP 429 rate-limiting responses.
- Cancellation token cleanly halts synchronization and leaves the local database in a consistent state.

## Comments

# Avalonia UI Catalog, Preview, and Selection Interface

Status: ready-for-agent

## Description

Implement the shared Avalonia UI in `CopilotChatExporter.UI` featuring the conversation catalog, full-text search, preview pane, and multi-selection workflow using MVVM.

## Scope of Work

1. Implement `CatalogViewModel` & `CatalogView`:
   - Virtualized list of conversations displaying title, update date, turn count, and export status badge.
   - Search bar with instant debounced filtering (utilizing SQLite FTS5).
   - Date range picker filter.
   - Selection mechanisms: individual checkboxes, "Select All", "Select None", "Select Unexported".
2. Implement `ConversationDetailViewModel` & `ConversationDetailView`:
   - Split-pane / detail view showing turns of the currently selected conversation.
   - User prompts rendered in distinct user bubbles; Copilot responses rendered with markdown styling and citation badges.
   - Clickable citation links opening original documents in system browser.
3. Implement `SyncStatusControl`:
   - Visual indicator showing current sync state (Idle, Syncing, Synced, Error).
   - "Sync Now" button triggering background synchronization.
4. Integrate Fluent Theme with automatic Dark/Light mode support.

## Acceptance Criteria

- UI remains responsive during large catalog scrolling and background sync.
- Search filters conversations instantly.
- Turns render clearly with proper visual distinction between prompts and responses.

## Comments

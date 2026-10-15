# Specification: M365 Copilot Chat Exporter

## 1. Overview & Objective

The **M365 Copilot Chat Exporter** is a multiplatform application built with **C# / .NET 8+** and **Avalonia UI** targeting **Desktop (Windows, macOS, Linux)** and **Mobile (iOS, Android)**. It empowers enterprise knowledge workers to authenticate against their Microsoft 365 account, index their **Conversation** history, preview dialogue **Turns**, and generate standalone **Export Bundles** in Markdown, offline HTML, and raw JSON formats.

All processing occurs strictly on the client device with **zero telemetry** and **zero administrative consent requirements**.

---

## 2. Architectural Boundaries

The solution adheres to Clean Layered Architecture across five projects:

1. **`CopilotChatExporter.Core`** (.NET 8+ Class Library)
   - Domain entities: `Conversation`, `Turn`, `Prompt`, `Response`, `Citation`.
   - Export engines: `IMarkdownExporter`, `IHtmlExporter`, `IJsonExporter`, `IIndexCatalogGenerator`.
   - Persistence abstractions: `ICatalogRepository`, `IExportHistoryRepository`.
   - Local database implementation: SQLite repository using `Microsoft.Data.Sqlite`.
   - Zero UI or operating-system-specific dependencies; 100% unit-testable.

2. **`CopilotChatExporter.Infrastructure`** (.NET 8+ Class Library)
   - Microsoft Substrate HTTP Client (`ISubstrateApiClient`): Manages authentication headers, conversation paging, and exponential backoff on HTTP 429.
   - Asset Downloader (`IAssetDownloader`): Downloads images/media from turns into a local `assets/` directory.
   - Platform Secure Storage (`ISecureStorage`): Cross-platform abstraction storing session credentials in native credential vaults (Keychain, Credential Locker, Secret Service, Android KeyStore).

3. **`CopilotChatExporter.UI`** (Avalonia Shared Library)
   - MVVM ViewModels powered by `CommunityToolkit.Mvvm`:
     - `CatalogViewModel`: Search, filtering, selection, and sorting of conversations.
     - `ConversationDetailViewModel`: Turn-by-turn preview with formatted Markdown, citations, and images.
     - `SyncViewModel`: Background synchronization progress and status indicators.
     - `ExportDialogViewModel`: Format selection (Markdown, HTML, JSON), destination selection, and export progress.
   - Reusable Avalonia XAML controls, themes (Light/Dark mode), and visual assets.

4. **`CopilotChatExporter.Desktop`** (Executable Project)
   - Platform entry point for Windows, macOS, and Linux.
   - Native folder picker integration via Avalonia's `IStorageProvider`.
   - Desktop window lifecycle and desktop-optimized layout.

5. **`CopilotChatExporter.Mobile`** (iOS & Android Project)
   - Platform entry points for iOS and Android.
   - Sandboxed file export: Packages the Export Bundle into a `.zip` archive and dispatches it via the native OS Share Sheet.
   - Touch-optimized responsive layout.

---

## 3. Domain Model & Storage Schema

### Domain Entities
- **Conversation**:
  - `Id` (string, unique remote identifier)
  - `Title` (string, sanitized title)
  - `CreatedAt` (DateTimeOffset)
  - `UpdatedAt` (DateTimeOffset)
  - `TurnCount` (int)
  - `LastExportedAt` (DateTimeOffset?, null if never exported)
  - `Turns` (List<Turn>)
- **Turn**:
  - `Id` (string)
  - `ConversationId` (string)
  - `TurnIndex` (int, 0-indexed order)
  - `Prompt` (string, user query)
  - `PromptTimestamp` (DateTimeOffset)
  - `Response` (string, Copilot synthesized response in Markdown)
  - `ResponseTimestamp` (DateTimeOffset)
  - `Citations` (List<Citation>)
  - `AssetUrls` (List<string>)
- **Citation**:
  - `Id` (string)
  - `Index` (int, 1-indexed citation number)
  - `Title` (string, document or web page title)
  - `Url` (string, absolute hyperlink to SharePoint, OneDrive, or web source)
  - `Snippet` (string, grounding text excerpt)

### SQLite Schema (`Microsoft.Data.Sqlite`)
- `conversations` (id TEXT PRIMARY KEY, title TEXT, created_at INTEGER, updated_at INTEGER, turn_count INTEGER, last_exported_at INTEGER)
- `turns` (id TEXT PRIMARY KEY, conversation_id TEXT, turn_index INTEGER, prompt TEXT, prompt_timestamp INTEGER, response TEXT, response_timestamp INTEGER, FOREIGN KEY(conversation_id) REFERENCES conversations(id))
- `citations` (id TEXT PRIMARY KEY, turn_id TEXT, citation_index INTEGER, title TEXT, url TEXT, snippet TEXT, FOREIGN KEY(turn_id) REFERENCES turns(id))
- `turns_fts` (FTS5 virtual table for instant full-text search across prompts and responses)

---

## 4. Authentication & Session Extraction

- **Mechanism**: The user initiates login via an embedded `Avalonia.WebView` window pointing to `https://m365.cloud.microsoft/chat`.
- **Interception**:
  - As the user completes Entra ID authentication (including SSO, MFA, and Conditional Access), the app intercepts HTTP requests directed toward `https://substrate.office.com/m365Copilot/`.
  - The app extracts the Bearer `Authorization` token and relevant authentication cookies.
- **Persistence**: Tokens and session cookies are serialized into encrypted OS storage (`ISecureStorage`).
- **Expiry Handling**: If API calls return HTTP 401 Unauthorized, the app silently re-authenticates using stored cookies or prompts a minimal webview refresh.

---

## 5. Substrate API Integration & Synchronization

- **Endpoints**:
  - Query conversations list: `GET https://substrate.office.com/m365Copilot/chats?$top={pageSize}&$skipToken={token}`
  - Query conversation details/turns: `GET https://substrate.office.com/m365Copilot/chats/{conversationId}/messages`
- **Rate-Limiting & Throttling**:
  - Controlled concurrency (max 2 parallel requests).
  - Explicit parsing of `Retry-After` headers on HTTP 429 with exponential backoff and jitter.
  - Cancellable operations via `CancellationToken`.
- **Sync Behavior**:
  - Fetches conversation metadata and compares with local SQLite records.
  - New and modified conversations are queued for message/turn retrieval and persisted locally.

---

## 6. Export Bundle Specifications

When exporting selected or all conversations, the engine produces:

1. **Markdown Files (`.md`)**:
   - Filename: `{YYYY-MM-DD} - {SanitizedTitle}.md`
   - YAML frontmatter:
     ```yaml
     ---
     id: "conv-123"
     title: "Quarterly Analysis Summary"
     created_at: 2026-10-15T10:30:00Z
     updated_at: 2026-10-15T11:00:00Z
     turns: 4
     ---
     ```
   - Content: Alternating `### User Prompt` and `### Copilot Response` blocks.
   - Citations: Formatted inline as `[^1]` with a `## Sources & Citations` reference list at the footer.
   - Images: Linked locally via `![Image](assets/{hash}.png)`.

2. **Self-Contained HTML Files (`.html`)**:
   - Filename: `{YYYY-MM-DD} - {SanitizedTitle}.html`
   - Structure: Clean, responsive layout with embedded CSS (`<style>`).
   - Theming: Automatic `@media (prefers-color-scheme: dark)` styling.
   - Air-Gapped: Zero external CDN script tags or remote font dependencies.
   - Features: Speech-bubble turn styling, collapsible citation drawers, and pre-formatted code blocks with syntax highlighting.

3. **Raw JSON Files (`.json`)**:
   - Filename: `{YYYY-MM-DD} - {SanitizedTitle}.json`
   - Full fidelity dump of all metadata, timestamps, turn payloads, and raw API responses.

4. **Master Index (`index.html`)**:
   - Summary catalog file located at the root of the Export Bundle.
   - Searchable/sortable table listing all exported conversations with metadata and direct links to the individual HTML and Markdown files.

5. **Assets Subfolder (`assets/`)**:
   - Contains all downloaded turn images, diagrams, and media attachments.

---

## 7. Delivery Workflows

- **Desktop (Windows, macOS, Linux)**:
  - User chooses a destination directory via native folder picker.
  - Files are written directly to disk.
  - Option to open the folder in File Explorer / Finder / file manager upon completion.
- **Mobile (iOS, Android)**:
  - Export Bundle is generated within the app sandbox cache.
  - Files are packaged into a single `.zip` file: `Copilot_Export_{YYYYMMDD_HHMMSS}.zip`.
  - Dispatched directly via the native OS Share Sheet (permitting "Save to Files", iCloud, Google Drive, AirDrop, etc.).

---

## 8. Privacy, Security & Compliance

- **Zero Outbound Telemetry**: No tracking SDKs (no Sentry, Google Analytics, AppCenter, etc.).
- **Zero Third-Party Relays**: Direct communication exclusively with Microsoft domain endpoints (`login.microsoftonline.com`, `substrate.office.com`, `m365.cloud.microsoft`).
- **Encrypted Local Storage**: Database and session tokens stored using platform secure storage facilities.

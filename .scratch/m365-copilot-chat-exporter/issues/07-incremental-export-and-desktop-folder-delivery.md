# 07: Incremental Export & Desktop Native Folder Delivery Slice

Status: ready-for-agent

**Blocked by:** 02: Self-Contained Offline HTML & Master Index Slice, 03: Raw JSON & Media Asset Downloader Slice, 06: Interactive Catalog, FTS5 Search, Filtering & Turn Preview Slice

## What to build
An end-to-end desktop export experience on Windows, macOS, and Linux that presents an export configuration dialog (format selection for Markdown, HTML, JSON, index, and assets), prompts the user for a destination folder using native OS folder pickers (`IStorageProvider`), writes the Export Bundle directly to disk, updates `LastExportedAt` timestamps in SQLite, and enables one-click incremental export of new or updated conversations.

## Acceptance Criteria

- [ ] **Export Configuration Dialog**:
  - Modal allows selecting output formats: [x] Markdown, [x] Offline HTML, [x] Raw JSON.
  - Option toggles for generating master `index.html` and downloading local media assets.
- [ ] **Desktop Native Folder Selection**:
  - Triggers Avalonia's `IStorageProvider.OpenFolderPickerAsync` allowing the user to select any local or network directory.
- [ ] **Direct Filesystem Output**:
  - Writes the complete Export Bundle directly into the chosen folder with subfolder structure (`assets/`, `.md`, `.html`, `.json`, `index.html`).
  - Real-time progress bar displays export progress (e.g. "Exporting 14 of 40 conversations...").
- [ ] **Export Tracking & Incremental Sync**:
  - Successfully exported conversations update their `LastExportedAt` timestamp in SQLite.
  - Catalog badges update immediately to reflect the new export status.
  - "Export New & Updated Since Last Export" button filters to only unexported or modified conversations.
- [ ] **Automated Seam Verification**:
  - Integration tests verify multi-format batch generation, directory file tree creation, and database timestamp updates.

## Verification
Run `dotnet test --filter "FullyQualifiedName~DesktopExportDeliveryTests"`.

# Desktop and Mobile Export Delivery Workflows

Status: ready-for-agent

## Description

Implement platform-specific export workflows: direct directory writing via native folder picker on Desktop (Windows, macOS, Linux), and zip packaging via native OS Share Sheet on Mobile (iOS, Android).

## Scope of Work

1. Implement `ExportDialogViewModel` & `ExportDialogView`:
   - Modal allowing format selection: Checkboxes for [x] Markdown, [x] Self-Contained HTML, [x] Raw JSON.
   - Option to include master `index.html` catalog.
   - Option to download referenced media assets into `assets/`.
   - Real-time export progress bar.
2. Implement Desktop Delivery in `CopilotChatExporter.Desktop`:
   - Use Avalonia's `IStorageProvider.OpenFolderPickerAsync` to prompt the user for target directory.
   - Write files directly to the selected directory.
   - Display success notification with action to reveal in file manager.
3. Implement Mobile Delivery in `CopilotChatExporter.Mobile`:
   - Generate export files in temporary application sandbox cache.
   - Package bundle into a single zip file (`Copilot_Export_{YYYYMMDD_HHMMSS}.zip`).
   - Trigger native OS Share Sheet (iOS `UIActivityViewController`, Android `ACTION_SEND` Intent).

## Acceptance Criteria

- Desktop correctly prompts for folder and writes the complete file tree directly.
- Mobile generates a valid `.zip` archive and triggers the native OS Share Sheet.
- Exported conversations have their `LastExportedAt` timestamp updated in SQLite.

## Comments

# Export Engine JSON Serializer and Asset Downloader

Status: ready-for-agent

## Description

Implement full-fidelity raw JSON export in `CopilotChatExporter.Core` and the local media/image asset downloader in `CopilotChatExporter.Infrastructure`.

## Scope of Work

1. Implement `IJsonExporter`:
   - Serialize conversation data into formatted JSON files (`{YYYY-MM-DD} - {SanitizedTitle}.json`).
   - Preserve complete structure including prompt text, raw response text, citation objects, timestamps, and turn sequence.
   - Use `System.Text.Json` with source-generated context or optimized options for performance.
2. Implement `IAssetDownloader`:
   - Identify image and media URLs referenced in Copilot responses.
   - Download binary assets into an `assets/` subfolder within the Export Bundle.
   - Compute SHA-256 or safe content hashes for filenames to avoid collision or duplication.
   - Handle network failures gracefully without failing the entire conversation export.
3. Unit tests verifying JSON serialization fidelity and asset download retry/error recovery.

## Acceptance Criteria

- JSON export accurately preserves 100% of the conversation turn data.
- Referenced images are successfully stored locally and linked relatively.
- Network errors during image retrieval log warnings but do not abort export.

## Comments

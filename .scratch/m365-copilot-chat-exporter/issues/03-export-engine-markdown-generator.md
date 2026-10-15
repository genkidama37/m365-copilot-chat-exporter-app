# Export Engine Markdown Generator

Status: ready-for-agent

## Description

Implement the Markdown file generator in `CopilotChatExporter.Core` to format conversations into clean GitHub Flavored Markdown files.

## Scope of Work

1. Implement `IMarkdownExporter`:
   - Generate standard YAML frontmatter containing `id`, `title`, `created_at`, `updated_at`, `turns`, and export metadata.
   - Format dialogue turns clearly distinguishing user prompts from Copilot responses (e.g. `### User Prompt` / `### Copilot Response`).
   - Format citations as numbered footnote markers in text (`[^1]`) linked to a concluding `## Sources & Citations` section with clickable URLs.
   - Replace remote image URLs with local relative paths (`assets/{filename}`) if media assets are present.
   - Sanitize output filenames following the pattern `{YYYY-MM-DD} - {SanitizedTitle}.md`.
2. Write unit tests covering:
   - YAML frontmatter serialization.
   - Citation footnote formatting.
   - Multiline markdown preservation, code blocks, tables, and special character sanitization.

## Acceptance Criteria

- Generated Markdown passes common GFM parsers and renders cleanly in Obsidian, GitHub, VS Code, and standard markdown viewers.
- Filename sanitization strips illegal OS characters (`/`, `\`, `:`, `*`, `?`, `"`, `<`, `>`, `|`).
- Unit tests verify 100% of formatting edge cases.

## Comments

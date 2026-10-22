# Deep Module Seams and Enforcing Boundaries with `internal`

The learner understands a **seam** as the junction where callers/tests meet a module's interface, and a **deep module seam** as placing a small public interface (`ICopilotExportService`) in front of substantial hidden implementation (`SqliteConversationRepository`, `ExportFileNameBuilder`, `HtmlExporter`, `MarkdownExporter`).

## Evidence
- Asked to unpack the terms "Primary seam", "Public seam", and "Deep module seam" from Lesson 0001.
- When presented with a hypothetical agent PR changing `ExportFileNameBuilder` from `internal` to `public` so the UI could call it directly, identified that doing so breaks the separation of concerns of the deep seam, creates extra seams to maintain, and causes duplicated orchestration logic across callers.

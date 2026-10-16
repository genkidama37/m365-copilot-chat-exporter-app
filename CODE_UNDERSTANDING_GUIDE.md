# Understanding the Generated Code with Agent Skills

Yes—both through the **paper trail** the skills have already left in this repo, and through several **active skills** you can run to inspect, audit, or be walked through the code.

## 1. Read the Paper Trail (Created by the Main Flow)

Because this repo uses the `/grill-with-docs` → `/to-spec` → `/to-tickets` → `/implement` flow, the "why" and "what" behind the code are already captured in primary-source files:

- **Domain Glossary (`CONTEXT.md`)**: [`CONTEXT.md`](CONTEXT.md) defines every core term used in the code ([`Conversation`](src/CopilotChatExporter.Core/Models/Conversation.cs), [`Turn`](src/CopilotChatExporter.Core/Models/Turn.cs), [`Citation`](src/CopilotChatExporter.Core/Models/Citation.cs), `Catalog`, `Export Bundle`, `Sync`, `Session`).
- **Architecture Decision Records (`docs/adr/`)**: Short records explaining the big structural choices:
  - [`0001-embedded-webview-session-extraction.md`](docs/adr/0001-embedded-webview-session-extraction.md)
  - [`0002-csharp-dotnet-multiplatform.md`](docs/adr/0002-csharp-dotnet-multiplatform.md)
  - [`0003-avalonia-ui-framework.md`](docs/adr/0003-avalonia-ui-framework.md)
  - [`0004-solution-architecture.md`](docs/adr/0004-solution-architecture.md)
  - [`0005-zero-telemetry-local-processing.md`](docs/adr/0005-zero-telemetry-local-processing.md)
- **Spec & Tracer-Bullet Tickets (`.scratch/`)**:
  - [`spec.md`](.scratch/m365-copilot-chat-exporter/spec.md) shows the end-to-end system design.
  - [`01-ingest-to-markdown-tracer-bullet.md`](.scratch/m365-copilot-chat-exporter/issues/01-ingest-to-markdown-tracer-bullet.md) is the exact slice currently implemented in [`CopilotExportService`](src/CopilotChatExporter.Core/Services/CopilotExportService.cs) and verified by [`CopilotExportServiceTests`](tests/CopilotChatExporter.Core.Tests/CopilotExportServiceTests.cs).

---

## 2. Skills You Can Run to Understand the Code

Depending on how you want to explore the code, reach for one of these:

| Goal | Skill to Use | What It Does |
| :--- | :--- | :--- |
| **See how the modules, seams, and architecture fit together** | **`/improve-codebase-architecture`** *(uses `/codebase-design`)* | Surveys the codebase for module boundaries, interfaces (like [`ICopilotExportService`](src/CopilotChatExporter.Core/Services/ICopilotExportService.cs)), internal details ([`SqliteConversationRepository`](src/CopilotChatExporter.Core/Persistence/SqliteConversationRepository.cs), [`MarkdownExporter`](src/CopilotChatExporter.Core/Exporters/MarkdownExporter.cs)), and coupling, and can generate an interactive HTML architecture report. |
| **Verify what the agent built against the spec & standards** | **`/code-review`** | Runs two parallel sub-agents to audit the code against **Standards** (ADRs, `CONTEXT.md` terminology) and **Spec** ([`01-ingest-to-markdown-tracer-bullet.md`](.scratch/m365-copilot-chat-exporter/issues/01-ingest-to-markdown-tracer-bullet.md)), giving you a side-by-side breakdown of what was built. |
| **Be guided through the code or stack step by step** | **`/teach`** | Sets up a structured learning flow tailored to your background (e.g., walking through the C# / .NET / Avalonia / SQLite architecture and how the pieces connect). |
| **Stop and get a plain-English explanation at any point** | **`/wait-what`** | If any explanation, class responsibility, or architectural decision doesn't land clearly, run `/wait-what` mid-conversation to get a plain-English re-pitch anchored in [`CONTEXT.md`](CONTEXT.md). |
| **Deep-dive a specific question into a saved doc** | **`/research`** | Delegates a background agent to trace a question through the codebase and primary sources and write a cited Markdown walkthrough in the repo. |

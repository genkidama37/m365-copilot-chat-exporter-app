# Mission: Reading, Reviewing, and Modifying the M365 Copilot Chat Exporter Codebase

## Why
You are building M365 Copilot Chat Exporter with AI coding agents in C# and .NET 10—a language and runtime you are new to. To stay in control of the architecture and quality, you need to read agent-generated C# with confidence, trace features end-to-end across seams, modify domain/repository/exporter code directly when needed, and catch subtle bugs or design smells during code review.

## Success looks like
- Trace any feature end-to-end from an xUnit test in `CopilotChatExporter.Core.Tests`, through `ICopilotExportService` / `CopilotExportService`, down to `SqliteConversationRepository` and the file exporters (`MarkdownExporter`, `HtmlExporter`).
- Write or modify C# domain models (`record` types), SQLite repository queries, and export formatters without assistance.
- Review agent-generated diffs and spot subtle bugs (such as async disposal leaks, N+1 SQLite queries, unescaped HTML/URI output, or broken relational integrity) and architectural smells (feature envy, duplicated code, leaky seams).

## Constraints
- Ground every lesson in the actual files, commits, and tests of this repository rather than generic C# toy examples.
- Bridge concepts from familiar languages (TypeScript, Python, Java) to modern C# / .NET 10 idioms.

## Out of scope
- Legacy .NET Framework history, Windows Forms, WPF, or enterprise ASP.NET boilerplate not used in this repository.
- Deep mobile native SDK internals (iOS Objective-C / Android Java bindings) before the core domain, persistence, and UI ViewModels are mastered.

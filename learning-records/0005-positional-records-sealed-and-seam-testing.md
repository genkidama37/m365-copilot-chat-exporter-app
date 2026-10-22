# Positional Record Expansion, `sealed`, and Seam-Based Testing

The learner unpacked five foundational C# and architectural concepts from Lesson 0001:
1. **Testing through the public seam (`ICopilotExportService`)**: Understood why `SqliteConversationRepository` and `HtmlExporter` are tested via `CopilotExportServiceTests` and `HtmlExporterTests` calling `harness.Service` rather than instantiating `internal` classes directly (preserving refactoring freedom and integration confidence).
2. **Positional `record` declarations**: Understood `public sealed record Citation(string Id, string Title, string Url);` via its concrete compiler expansion into `{ get; init; }` properties, a positional constructor, and a `Deconstruct` method.
3. **`sealed` modifier**: Understood `sealed` (like Java's `final class`) as forbidding inheritance to enforce interface seams over subclassing, protect record value equality, and enable JIT devirtualization.
4. **Copy-and-update via `with` expressions**: Understood `conversation with { Title = "New" }` via its TypeScript (`{ ...conversation, title: "New" }`) and compiler copy-constructor equivalents.
5. **`.csproj` files and Compiled Assemblies (`.dll`)**: Understood that a `.csproj` file combines dependency + compiler configuration (like `package.json` + `tsconfig.json`) and compiles its directory's `.cs` files into a single assembly (`.dll`), which is the boundary enforced by the `internal` visibility modifier.

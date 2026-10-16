# C# / .NET 10 & Clean Architecture Resources

## Knowledge

- [Microsoft Learn: C# Records Reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
  Official C# language reference for positional `record` types, value-based equality, and immutability. Use for: understanding `Conversation`, `Turn`, and `Citation` in `src/CopilotChatExporter.Core/Models/`.
- [Microsoft Learn: Asynchronous programming with async and await](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
  Primary guide to the Task Asynchronous Pattern (`Task`, `Task<T>`, `ValueTask`, `CancellationToken`, and `await using` for `IAsyncDisposable`). Use for: tracing `ICopilotExportService` and `TestServiceHarness`.
- [Microsoft Learn: Microsoft.Data.Sqlite Overview](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)
  Official documentation for Microsoft's lightweight ADO.NET provider for SQLite, including in-memory databases (`Filename=:memory:`), parameterized queries, and transactions. Use for: reading and modifying `SqliteConversationRepository.cs`.
- [xUnit.net Documentation: Getting Started & Shared Context](https://xunit.net/docs/shared-context)
  Primary reference from the authors of xUnit on test lifecycle (`[Fact]`, `[Theory]`, and fixture disposal). Use for: reading and writing seam tests in `tests/CopilotChatExporter.Core.Tests/`.
- [Book: _A Philosophy of Software Design_ by John Ousterhout](https://web.stanford.edu/~ouster/cgi-bin/book.php)
  Foundational text on deep modules vs. shallow modules, information hiding, and interface simplicity. Use for: evaluating whether `ICopilotExportService` and internal helpers have the right seam boundaries.
- [Book: _Refactoring: Improving the Design of Existing Code_ (2nd Ed.) by Martin Fowler](https://martinfowler.com/books/refactoring.html)
  Canonical catalogue of code smells (Duplicated Code, Feature Envy, Divergent Change, Shotgun Surgery). Use for: reviewing agent-generated diffs on the Standards axis.

## Wisdom (Communities)

- [r/csharp](https://www.reddit.com/r/csharp/)
  High-signal community of practicing C# engineers. Use for: idiomatic modern C# questions, code review feedback, and .NET library trade-offs.
- [r/dotnet](https://www.reddit.com/r/dotnet/)
  Practitioner community focused on the broader .NET runtime, tooling, performance, and cross-platform development. Use for: .NET 10 SDK, AOT/self-contained publishing, and SQLite/Avalonia ecosystem questions.
- [Avalonia UI GitHub Discussions](https://github.com/AvaloniaUI/Avalonia/discussions)
  Official forum frequented by Avalonia core maintainers and cross-platform desktop/mobile developers. Use for: XAML data binding, MVVM patterns, and platform-specific storage/webview questions.

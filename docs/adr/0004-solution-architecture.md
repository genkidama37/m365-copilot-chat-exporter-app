# Clean Layered Architecture for Multiplatform .NET Solution

We structure the C# / .NET solution into a clean layered architecture separating domain logic, infrastructure concerns, and user interfaces across Desktop and Mobile heads.

## Status

Accepted

## Context and Decision

To support Windows, macOS, Linux, iOS, and Android while ensuring maximum testability and maintainability, the solution is partitioned into:
- `CopilotChatExporter.Core`: Pure .NET 8+ class library containing domain models (`Conversation`, `Turn`, `Citation`), file exporters (Markdown, HTML, JSON), SQLite repositories, and domain abstractions. Completely decoupled from UI and OS APIs.
- `CopilotChatExporter.Infrastructure`: Implements Substrate HTTP clients, webview token extraction, and OS-specific secure storage adapters (Keychain, Credential Locker, Secret Service).
- `CopilotChatExporter.UI`: Shared Avalonia XAML views and ViewModels using `CommunityToolkit.Mvvm`.
- `CopilotChatExporter.Desktop`: Executable target for Windows, macOS, and Linux.
- `CopilotChatExporter.Mobile`: Executable targets for iOS and Android.

## Consequences

Domain parsing, export formatting, and database queries can be unit-tested without launching UI or mocking platform-specific webview handles.

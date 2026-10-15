# Scaffold Solution and Project Structure

Status: ready-for-agent

## Description

Initialize the .NET 8+ multiplatform solution structure with clean architectural boundaries for Desktop and Mobile platforms using Avalonia UI.

## Scope of Work

1. Create root solution file `CopilotChatExporter.sln`.
2. Create class library projects:
   - `src/CopilotChatExporter.Core`: Pure domain abstractions, entities, export generators, and SQLite storage.
   - `src/CopilotChatExporter.Infrastructure`: Substrate HTTP client, webview token extraction, and secure credential storage.
   - `src/CopilotChatExporter.UI`: Shared Avalonia XAML controls, views, and ViewModels (`CommunityToolkit.Mvvm`).
3. Create executable projects:
   - `src/CopilotChatExporter.Desktop`: Entry point for Windows, macOS, and Linux.
   - `src/CopilotChatExporter.Mobile`: Entry point for iOS and Android.
4. Create test project:
   - `tests/CopilotChatExporter.Core.Tests`: Unit tests for domain models, serialization, and export formatting.
5. Configure package references (`Avalonia`, `Avalonia.Themes.Fluent`, `Microsoft.Data.Sqlite`, `CommunityToolkit.Mvvm`).

## Acceptance Criteria

- Solution builds cleanly across `dotnet build`.
- Dependencies between projects strictly adhere to Clean Architecture (`Core` has zero dependencies on `Infrastructure` or `UI`).
- Test project executes via `dotnet test`.

## Comments

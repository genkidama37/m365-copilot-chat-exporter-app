# M365 Copilot Chat Exporter

A multiplatform desktop and mobile application built with C#, .NET 10, and Avalonia UI that enables Microsoft 365 Copilot enterprise users to browse, search, and export their conversation history into readable and archival formats (GitHub Flavored Markdown, self-contained offline HTML, and raw JSON).

## Solution Structure

- `src/CopilotChatExporter.Core` — Domain models (`Conversation`, `Turn`, `Citation`), SQLite persistence, and export formatters (zero UI/OS dependencies)
- `src/CopilotChatExporter.Infrastructure` — Microsoft Substrate HTTP client, session interception, and platform secure storage adapters
- `src/CopilotChatExporter.UI` — Shared Avalonia XAML views and `CommunityToolkit.Mvvm` ViewModels
- `src/CopilotChatExporter.Desktop` — Desktop executable head (Windows, macOS, Linux)
- `src/CopilotChatExporter.Mobile` — Mobile application target (iOS, Android)
- `tests/CopilotChatExporter.Core.Tests` — Automated xUnit integration and unit test suite

---

## Prerequisites

Install the **.NET 10.0 SDK** (`10.0.100` or later).

### macOS

Using [Homebrew](https://brew.sh/):

```bash
brew install dotnet
```

If needed, add the Homebrew .NET binary directory to your shell profile (`~/.zshrc`):

```bash
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
export PATH="$PATH:/opt/homebrew/bin"
```

Verify the installation:

```bash
dotnet --version
```

### Windows

Using **winget** (PowerShell or Command Prompt):

```powershell
winget install Microsoft.DotNet.SDK.10
```

Or download the `.NET 10.0 SDK` installer directly from [dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0).

Verify the installation:

```powershell
dotnet --version
```

---

## Building the Application

From the repository root on either **Windows** or **macOS**:

```bash
dotnet build CopilotChatExporter.sln
```

To build in `Release` configuration:

```bash
dotnet build CopilotChatExporter.sln -c Release
```

---

## Running the Desktop App

```bash
dotnet run --project src/CopilotChatExporter.Desktop/CopilotChatExporter.Desktop.csproj
```

---

## Publishing Self-Contained Binaries

### macOS (Apple Silicon `osx-arm64` / Intel `osx-x64`)

```bash
# Apple Silicon (M1/M2/M3/M4)
dotnet publish src/CopilotChatExporter.Desktop/CopilotChatExporter.Desktop.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained true \
  -o ./publish/macos-arm64

# Intel Mac
dotnet publish src/CopilotChatExporter.Desktop/CopilotChatExporter.Desktop.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  -o ./publish/macos-x64
```

### Windows (`win-x64` / `win-arm64`)

```powershell
# Windows x64
dotnet publish src/CopilotChatExporter.Desktop/CopilotChatExporter.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o ./publish/win-x64

# Windows ARM64
dotnet publish src/CopilotChatExporter.Desktop/CopilotChatExporter.Desktop.csproj `
  -c Release `
  -r win-arm64 `
  --self-contained true `
  -o ./publish/win-arm64
```

---

## Running Tests

Run the full xUnit test suite:

```bash
dotnet test CopilotChatExporter.sln
```

Or target only the core test project:

```bash
dotnet test tests/CopilotChatExporter.Core.Tests
```

---

## License

This project is licensed under the [Mozilla Public License 2.0 (MPL-2.0)](LICENSE).


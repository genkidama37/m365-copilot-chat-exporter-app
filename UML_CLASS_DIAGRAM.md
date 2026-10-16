# UML Class Diagram

Here is the UML class diagram of the current codebase across `CopilotChatExporter.Core`, `CopilotChatExporter.UI`, `CopilotChatExporter.Desktop`, and `CopilotChatExporter.Mobile`:

```mermaid
classDiagram
    direction TB

    namespace CopilotChatExporter_Core_Models {
        class Conversation {
            <<record>>
            +string Id
            +string Title
            +DateTimeOffset CreatedAt
            +DateTimeOffset UpdatedAt
            +IReadOnlyList~Turn~ Turns
        }

        class Turn {
            <<record>>
            +string Id
            +int SequenceNumber
            +string Prompt
            +string Response
            +DateTimeOffset CreatedAt
            +IReadOnlyList~Citation~ Citations
        }

        class Citation {
            <<record>>
            +string Id
            +string Title
            +string Url
        }
    }

    namespace CopilotChatExporter_Core_Services {
        class ICopilotExportService {
            <<interface>>
            +InitializeAsync(CancellationToken) Task
            +IngestConversationAsync(Conversation, CancellationToken) Task
            +GetConversationAsync(string, CancellationToken) Task~Conversation?~
            +ExportConversationToMarkdownAsync(string, string, CancellationToken) Task~string~
            +ExportConversationToHtmlAsync(string, string, CancellationToken) Task~string~
            +ExportMasterIndexAsync(string, CancellationToken) Task~string~
        }

        class CopilotExportService {
            -UTF8Encoding Utf8WithoutBom$
            -SqliteConversationRepository _repository
            +CopilotExportService(SqliteConnection)
            +InitializeAsync(CancellationToken) Task
            +IngestConversationAsync(Conversation, CancellationToken) Task
            +GetConversationAsync(string, CancellationToken) Task~Conversation?~
            +ExportConversationToMarkdownAsync(string, string, CancellationToken) Task~string~
            +ExportConversationToHtmlAsync(string, string, CancellationToken) Task~string~
            +ExportMasterIndexAsync(string, CancellationToken) Task~string~
            -ExportConversationAsync(string, string, string, Func~Conversation, string~, CancellationToken) Task~string~
            -HasExportedFilesInDirectory(Conversation, string)$ bool
        }
    }

    namespace CopilotChatExporter_Core_Persistence {
        class SqliteConversationRepository {
            ~internal~
            -SqliteConnection _connection
            +SqliteConversationRepository(SqliteConnection)
            +InitializeSchemaAsync(CancellationToken) Task
            +SaveConversationAsync(Conversation, CancellationToken) Task
            +GetConversationAsync(string, CancellationToken) Task~Conversation?~
            +GetAllConversationsAsync(CancellationToken) Task~IReadOnlyList~Conversation~~
            -EnableForeignKeysAsync(CancellationToken) Task
        }
    }

    namespace CopilotChatExporter_Core_Exporters {
        class ExportFileNameBuilder {
            <<static, internal>>
            -HashSet~char~ IllegalFileNameCharacters$
            +BuildSanitizedFileName(DateTimeOffset, string, string)$ string
        }

        class MarkdownExporter {
            <<static, internal>>
            +FormatConversation(Conversation)$ string
            -EscapeYamlString(string)$ string
        }

        class HtmlExporter {
            <<static, internal>>
            -string SharedBaseStyles$
            +FormatConversation(Conversation)$ string
            +FormatMasterIndex(IReadOnlyList~Conversation~)$ string
            -AppendDocumentHeader(StringBuilder, string, string)$ void
            -RenderMarkdownBlocks(string, string)$ string
            -EncodeRelativeHref(string)$ string
            -HtmlEncode(string)$ string
        }
    }

    namespace CopilotChatExporter_UI {
        class App {
            +Initialize() void
            +OnFrameworkInitializationCompleted() void
        }

        class MainWindow {
            +MainWindow()
        }

        class MainView {
            +MainView()
        }
    }

    namespace CopilotChatExporter_Heads {
        class Program {
            ~Desktop, internal~
            +Main(string[])$ void
            +BuildAvaloniaApp()$ AppBuilder
        }

        class MobileAppBuilder {
            <<Mobile, static>>
            +BuildAvaloniaApp()$ AppBuilder
        }
    }

    %% Domain Model Relationships
    Conversation "1" *-- "0..*" Turn : Turns
    Turn "1" *-- "0..*" Citation : Citations

    %% Service, Persistence & Exporter Relationships
    ICopilotExportService <|.. CopilotExportService
    CopilotExportService *-- "1" SqliteConversationRepository : _repository
    CopilotExportService ..> ExportFileNameBuilder : uses
    CopilotExportService ..> MarkdownExporter : uses
    CopilotExportService ..> HtmlExporter : uses
    CopilotExportService ..> Conversation : ingests / retrieves
    SqliteConversationRepository ..> Conversation : persists / loads
    SqliteConversationRepository ..> Turn : persists / loads
    SqliteConversationRepository ..> Citation : persists / loads
    MarkdownExporter ..> Conversation : formats
    MarkdownExporter ..> Turn : reads
    MarkdownExporter ..> Citation : formats footnotes
    HtmlExporter ..> ExportFileNameBuilder : uses
    HtmlExporter ..> Conversation : formats
    HtmlExporter ..> Turn : reads
    HtmlExporter ..> Citation : formats footnotes

    %% UI & Platform Head Relationships
    Program ..> App : configures
    MobileAppBuilder ..> App : configures
    App ..> MainWindow : creates (desktop)
    App ..> MainView : creates (single-view)
    MainWindow *-- "1" MainView : hosts in AXAML
```

## Summary of Components

- **Domain Models (`CopilotChatExporter.Core.Models`)**
  - [`Conversation`](src/CopilotChatExporter.Core/Models/Conversation.cs): Root aggregate record holding metadata and an ordered list of [`Turn`](src/CopilotChatExporter.Core/Models/Turn.cs) records.
  - [`Turn`](src/CopilotChatExporter.Core/Models/Turn.cs): Immutable record representing a prompt/response exchange and its attached [`Citation`](src/CopilotChatExporter.Core/Models/Citation.cs) list.
  - [`Citation`](src/CopilotChatExporter.Core/Models/Citation.cs): Grounding reference record (`Id`, `Title`, `Url`).
- **Services, Persistence & Exporters (`CopilotChatExporter.Core`)**
  - [`ICopilotExportService`](src/CopilotChatExporter.Core/Services/ICopilotExportService.cs) / [`CopilotExportService`](src/CopilotChatExporter.Core/Services/CopilotExportService.cs): Public service seam coordinating SQLite persistence via [`SqliteConversationRepository`](src/CopilotChatExporter.Core/Persistence/SqliteConversationRepository.cs), filename sanitization via [`ExportFileNameBuilder`](src/CopilotChatExporter.Core/Exporters/ExportFileNameBuilder.cs), Markdown generation via [`MarkdownExporter`](src/CopilotChatExporter.Core/Exporters/MarkdownExporter.cs), and standalone offline HTML / Master Index catalog generation via [`HtmlExporter`](src/CopilotChatExporter.Core/Exporters/HtmlExporter.cs).
- **UI & Platform Entrypoints (`CopilotChatExporter.UI`, `Desktop`, `Mobile`)**
  - [`Program`](src/CopilotChatExporter.Desktop/Program.cs) and [`MobileAppBuilder`](src/CopilotChatExporter.Mobile/MobileAppBuilder.cs) bootstrap the Avalonia [`App`](src/CopilotChatExporter.UI/App.axaml.cs), which initializes [`MainWindow`](src/CopilotChatExporter.UI/Views/MainWindow.axaml.cs) (desktop) or [`MainView`](src/CopilotChatExporter.UI/Views/MainView.axaml.cs) (mobile/single-view).

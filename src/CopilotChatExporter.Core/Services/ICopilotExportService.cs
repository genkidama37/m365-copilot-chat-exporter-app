using CopilotChatExporter.Core.Models;

namespace CopilotChatExporter.Core.Services;

/// <summary>
/// Primary application service seam orchestrating conversation ingestion, persistence, and export.
/// </summary>
public interface ICopilotExportService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task IngestConversationAsync(Conversation conversation, CancellationToken cancellationToken = default);

    Task<Conversation?> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    Task<string> ExportConversationToMarkdownAsync(
        string conversationId,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}

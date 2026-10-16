using System.Text;
using CopilotChatExporter.Core.Exporters;
using CopilotChatExporter.Core.Models;
using CopilotChatExporter.Core.Persistence;
using Microsoft.Data.Sqlite;

namespace CopilotChatExporter.Core.Services;

public sealed class CopilotExportService : ICopilotExportService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private readonly SqliteConversationRepository _repository;

    public CopilotExportService(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _repository = new SqliteConversationRepository(connection);
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => _repository.InitializeSchemaAsync(cancellationToken);

    public Task IngestConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
        => _repository.SaveConversationAsync(conversation, cancellationToken);

    public Task<Conversation?> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default)
        => _repository.GetConversationAsync(conversationId, cancellationToken);

    public async Task<string> ExportConversationToMarkdownAsync(
        string conversationId,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var conversation = await _repository.GetConversationAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        Directory.CreateDirectory(outputDirectory);

        var fileName = MarkdownExporter.BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, ".md");
        var filePath = Path.Combine(outputDirectory, fileName);
        var markdown = MarkdownExporter.FormatConversation(conversation);

        await File.WriteAllTextAsync(filePath, markdown, Utf8WithoutBom, cancellationToken);
        return filePath;
    }
}

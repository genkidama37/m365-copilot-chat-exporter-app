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

    public Task<string> ExportConversationToMarkdownAsync(
        string conversationId,
        string outputDirectory,
        CancellationToken cancellationToken = default)
        => ExportConversationAsync(
            conversationId,
            outputDirectory,
            ".md",
            MarkdownExporter.FormatConversation,
            cancellationToken);

    public Task<string> ExportConversationToHtmlAsync(
        string conversationId,
        string outputDirectory,
        CancellationToken cancellationToken = default)
        => ExportConversationAsync(
            conversationId,
            outputDirectory,
            ".html",
            HtmlExporter.FormatConversation,
            cancellationToken);

    public async Task<string> ExportMasterIndexAsync(
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var allConversations = await _repository.GetAllConversationsAsync(cancellationToken);
        var exportedConversations = allConversations
            .Where(c => HasExportedFilesInDirectory(c, outputDirectory))
            .ToList();

        var conversationsToIndex = exportedConversations.Count > 0
            ? exportedConversations
            : allConversations;

        var indexPath = Path.Combine(outputDirectory, "index.html");
        var indexHtml = HtmlExporter.FormatMasterIndex(conversationsToIndex);

        await File.WriteAllTextAsync(indexPath, indexHtml, Utf8WithoutBom, cancellationToken);
        return indexPath;
    }

    private async Task<string> ExportConversationAsync(
        string conversationId,
        string outputDirectory,
        string extension,
        Func<Conversation, string> formatter,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var conversation = await _repository.GetConversationAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        Directory.CreateDirectory(outputDirectory);

        var fileName = ExportFileNameBuilder.BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, extension);
        var filePath = Path.Combine(outputDirectory, fileName);
        var content = formatter(conversation);

        await File.WriteAllTextAsync(filePath, content, Utf8WithoutBom, cancellationToken);
        return filePath;
    }

    private static bool HasExportedFilesInDirectory(Conversation conversation, string outputDirectory)
    {
        var htmlPath = Path.Combine(
            outputDirectory,
            ExportFileNameBuilder.BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, ".html"));
        var mdPath = Path.Combine(
            outputDirectory,
            ExportFileNameBuilder.BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, ".md"));

        return File.Exists(htmlPath) || File.Exists(mdPath);
    }
}

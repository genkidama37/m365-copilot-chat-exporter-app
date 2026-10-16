using System.Globalization;
using System.Text;
using CopilotChatExporter.Core.Models;
using Microsoft.Data.Sqlite;

namespace CopilotChatExporter.Core.Services;

public sealed class CopilotExportService : ICopilotExportService
{
    private static readonly HashSet<char> IllegalFileNameCharacters =
    [
        '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\0'
    ];

    private readonly SqliteConnection _connection;

    public CopilotExportService(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS conversations (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS turns (
                id TEXT PRIMARY KEY,
                conversation_id TEXT NOT NULL,
                sequence_number INTEGER NOT NULL,
                prompt TEXT NOT NULL,
                response TEXT NOT NULL,
                created_at TEXT NOT NULL,
                FOREIGN KEY (conversation_id) REFERENCES conversations(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS citations (
                id TEXT PRIMARY KEY,
                turn_id TEXT NOT NULL,
                sequence_number INTEGER NOT NULL,
                title TEXT NOT NULL,
                url TEXT NOT NULL,
                FOREIGN KEY (turn_id) REFERENCES turns(id) ON DELETE CASCADE
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task IngestConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        await using var transaction = (SqliteTransaction)await _connection.BeginTransactionAsync(cancellationToken);

        await using (var deleteExistingCmd = _connection.CreateCommand())
        {
            deleteExistingCmd.Transaction = transaction;
            deleteExistingCmd.CommandText = "DELETE FROM conversations WHERE id = $id;";
            deleteExistingCmd.Parameters.AddWithValue("$id", conversation.Id);
            await deleteExistingCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var insertConversationCmd = _connection.CreateCommand())
        {
            insertConversationCmd.Transaction = transaction;
            insertConversationCmd.CommandText = """
                INSERT INTO conversations (id, title, created_at, updated_at)
                VALUES ($id, $title, $created_at, $updated_at);
                """;
            insertConversationCmd.Parameters.AddWithValue("$id", conversation.Id);
            insertConversationCmd.Parameters.AddWithValue("$title", conversation.Title);
            insertConversationCmd.Parameters.AddWithValue("$created_at", conversation.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
            insertConversationCmd.Parameters.AddWithValue("$updated_at", conversation.UpdatedAt.ToString("O", CultureInfo.InvariantCulture));
            await insertConversationCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var turn in conversation.Turns)
        {
            await using (var insertTurnCmd = _connection.CreateCommand())
            {
                insertTurnCmd.Transaction = transaction;
                insertTurnCmd.CommandText = """
                    INSERT INTO turns (id, conversation_id, sequence_number, prompt, response, created_at)
                    VALUES ($id, $conversation_id, $sequence_number, $prompt, $response, $created_at);
                    """;
                insertTurnCmd.Parameters.AddWithValue("$id", turn.Id);
                insertTurnCmd.Parameters.AddWithValue("$conversation_id", conversation.Id);
                insertTurnCmd.Parameters.AddWithValue("$sequence_number", turn.SequenceNumber);
                insertTurnCmd.Parameters.AddWithValue("$prompt", turn.Prompt);
                insertTurnCmd.Parameters.AddWithValue("$response", turn.Response);
                insertTurnCmd.Parameters.AddWithValue("$created_at", turn.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
                await insertTurnCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            for (var index = 0; index < turn.Citations.Count; index++)
            {
                var citation = turn.Citations[index];
                await using var insertCitationCmd = _connection.CreateCommand();
                insertCitationCmd.Transaction = transaction;
                insertCitationCmd.CommandText = """
                    INSERT INTO citations (id, turn_id, sequence_number, title, url)
                    VALUES ($id, $turn_id, $sequence_number, $title, $url);
                    """;
                insertCitationCmd.Parameters.AddWithValue("$id", citation.Id);
                insertCitationCmd.Parameters.AddWithValue("$turn_id", turn.Id);
                insertCitationCmd.Parameters.AddWithValue("$sequence_number", index + 1);
                insertCitationCmd.Parameters.AddWithValue("$title", citation.Title);
                insertCitationCmd.Parameters.AddWithValue("$url", citation.Url);
                await insertCitationCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<Conversation?> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        string id;
        string title;
        DateTimeOffset createdAt;
        DateTimeOffset updatedAt;

        await using (var convCmd = _connection.CreateCommand())
        {
            convCmd.CommandText = """
                SELECT id, title, created_at, updated_at
                FROM conversations
                WHERE id = $id;
                """;
            convCmd.Parameters.AddWithValue("$id", conversationId);

            await using var reader = await convCmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            id = reader.GetString(0);
            title = reader.GetString(1);
            createdAt = DateTimeOffset.Parse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            updatedAt = DateTimeOffset.Parse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        var turns = new List<Turn>();
        await using (var turnsCmd = _connection.CreateCommand())
        {
            turnsCmd.CommandText = """
                SELECT id, sequence_number, prompt, response, created_at
                FROM turns
                WHERE conversation_id = $conversation_id
                ORDER BY sequence_number ASC;
                """;
            turnsCmd.Parameters.AddWithValue("$conversation_id", conversationId);

            await using var turnsReader = await turnsCmd.ExecuteReaderAsync(cancellationToken);
            while (await turnsReader.ReadAsync(cancellationToken))
            {
                var turnId = turnsReader.GetString(0);
                var sequenceNumber = turnsReader.GetInt32(1);
                var prompt = turnsReader.GetString(2);
                var response = turnsReader.GetString(3);
                var turnCreatedAt = DateTimeOffset.Parse(turnsReader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                var citations = new List<Citation>();
                await using (var citationsCmd = _connection.CreateCommand())
                {
                    citationsCmd.CommandText = """
                        SELECT id, title, url
                        FROM citations
                        WHERE turn_id = $turn_id
                        ORDER BY sequence_number ASC;
                        """;
                    citationsCmd.Parameters.AddWithValue("$turn_id", turnId);

                    await using var citationsReader = await citationsCmd.ExecuteReaderAsync(cancellationToken);
                    while (await citationsReader.ReadAsync(cancellationToken))
                    {
                        citations.Add(new Citation(
                            Id: citationsReader.GetString(0),
                            Title: citationsReader.GetString(1),
                            Url: citationsReader.GetString(2)));
                    }
                }

                turns.Add(new Turn(
                    Id: turnId,
                    SequenceNumber: sequenceNumber,
                    Prompt: prompt,
                    Response: response,
                    CreatedAt: turnCreatedAt,
                    Citations: citations));
            }
        }

        return new Conversation(
            Id: id,
            Title: title,
            CreatedAt: createdAt,
            UpdatedAt: updatedAt,
            Turns: turns);
    }

    public async Task<string> ExportConversationToMarkdownAsync(
        string conversationId,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var conversation = await GetConversationAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        Directory.CreateDirectory(outputDirectory);

        var fileName = BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, ".md");
        var filePath = Path.Combine(outputDirectory, fileName);
        var markdown = FormatConversationMarkdown(conversation);

        await File.WriteAllTextAsync(filePath, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);
        return filePath;
    }

    private static string BuildSanitizedFileName(DateTimeOffset createdAt, string title, string extension)
    {
        var datePrefix = createdAt.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var sanitizedBuilder = new StringBuilder(title.Length);
        var previousWasWhitespace = false;

        foreach (var ch in title)
        {
            if (char.IsControl(ch) || IllegalFileNameCharacters.Contains(ch))
            {
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace && sanitizedBuilder.Length > 0)
                {
                    sanitizedBuilder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            sanitizedBuilder.Append(ch);
            previousWasWhitespace = false;
        }

        var sanitizedTitle = sanitizedBuilder.ToString().Trim().TrimEnd('.');
        if (string.IsNullOrWhiteSpace(sanitizedTitle))
        {
            sanitizedTitle = "Untitled Conversation";
        }

        return $"{datePrefix} - {sanitizedTitle}{extension}";
    }

    private static string FormatConversationMarkdown(Conversation conversation)
    {
        var sb = new StringBuilder();
        sb.Append("---\n");
        sb.Append($"id: \"{EscapeYamlString(conversation.Id)}\"\n");
        sb.Append($"title: \"{EscapeYamlString(conversation.Title)}\"\n");
        sb.Append($"created_at: \"{conversation.CreatedAt.ToString("O", CultureInfo.InvariantCulture)}\"\n");
        sb.Append($"updated_at: \"{conversation.UpdatedAt.ToString("O", CultureInfo.InvariantCulture)}\"\n");
        sb.Append($"turns: {conversation.Turns.Count.ToString(CultureInfo.InvariantCulture)}\n");
        sb.Append("---\n\n");
        sb.Append($"# {conversation.Title}\n");

        var footnotes = new List<(int Number, Citation Citation)>();
        var nextFootnoteNumber = 1;

        foreach (var turn in conversation.Turns)
        {
            sb.Append($"\n## Turn {turn.SequenceNumber.ToString(CultureInfo.InvariantCulture)}\n\n");
            sb.Append("### Prompt\n\n");
            sb.Append($"{turn.Prompt}\n\n");
            sb.Append("### Response\n\n");
            sb.Append(turn.Response);

            foreach (var citation in turn.Citations)
            {
                var footnoteNumber = nextFootnoteNumber++;
                footnotes.Add((footnoteNumber, citation));
                sb.Append($" [^{footnoteNumber.ToString(CultureInfo.InvariantCulture)}]");
            }

            sb.Append('\n');
        }

        if (footnotes.Count > 0)
        {
            sb.Append("\n## Sources & Citations\n\n");
            foreach (var (number, citation) in footnotes)
            {
                sb.Append($"[^{number.ToString(CultureInfo.InvariantCulture)}]: [{citation.Title}]({citation.Url})\n");
            }
        }

        return sb.ToString();
    }

    private static string EscapeYamlString(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}

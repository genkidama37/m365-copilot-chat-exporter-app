using System.Globalization;
using CopilotChatExporter.Core.Models;
using Microsoft.Data.Sqlite;

namespace CopilotChatExporter.Core.Persistence;

internal sealed class SqliteConversationRepository
{
    private readonly SqliteConnection _connection;

    public SqliteConversationRepository(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
    }

    public async Task InitializeSchemaAsync(CancellationToken cancellationToken = default)
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
                id TEXT NOT NULL,
                turn_id TEXT NOT NULL,
                sequence_number INTEGER NOT NULL,
                title TEXT NOT NULL,
                url TEXT NOT NULL,
                PRIMARY KEY (turn_id, id),
                FOREIGN KEY (turn_id) REFERENCES turns(id) ON DELETE CASCADE
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        await EnableForeignKeysAsync(cancellationToken);
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

    public async Task<IReadOnlyList<Conversation>> GetAllConversationsAsync(CancellationToken cancellationToken = default)
    {
        var conversationRows = new List<(string Id, string Title, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)>();
        await using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = """
                SELECT id, title, created_at, updated_at
                FROM conversations
                ORDER BY created_at DESC, id ASC;
                """;
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                conversationRows.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    DateTimeOffset.Parse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTimeOffset.Parse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)));
            }
        }

        if (conversationRows.Count == 0)
        {
            return [];
        }

        var citationsByTurnId = new Dictionary<string, List<Citation>>(StringComparer.Ordinal);
        await using (var citationsCmd = _connection.CreateCommand())
        {
            citationsCmd.CommandText = """
                SELECT turn_id, id, title, url
                FROM citations
                ORDER BY turn_id ASC, sequence_number ASC;
                """;
            await using var citationsReader = await citationsCmd.ExecuteReaderAsync(cancellationToken);
            while (await citationsReader.ReadAsync(cancellationToken))
            {
                var turnId = citationsReader.GetString(0);
                if (!citationsByTurnId.TryGetValue(turnId, out var list))
                {
                    list = [];
                    citationsByTurnId[turnId] = list;
                }

                list.Add(new Citation(
                    Id: citationsReader.GetString(1),
                    Title: citationsReader.GetString(2),
                    Url: citationsReader.GetString(3)));
            }
        }

        var turnsByConversationId = new Dictionary<string, List<Turn>>(StringComparer.Ordinal);
        await using (var turnsCmd = _connection.CreateCommand())
        {
            turnsCmd.CommandText = """
                SELECT conversation_id, id, sequence_number, prompt, response, created_at
                FROM turns
                ORDER BY conversation_id ASC, sequence_number ASC;
                """;
            await using var turnsReader = await turnsCmd.ExecuteReaderAsync(cancellationToken);
            while (await turnsReader.ReadAsync(cancellationToken))
            {
                var convId = turnsReader.GetString(0);
                var turnId = turnsReader.GetString(1);
                if (!turnsByConversationId.TryGetValue(convId, out var list))
                {
                    list = [];
                    turnsByConversationId[convId] = list;
                }

                var citations = citationsByTurnId.TryGetValue(turnId, out var turnCitations)
                    ? (IReadOnlyList<Citation>)turnCitations
                    : [];

                list.Add(new Turn(
                    Id: turnId,
                    SequenceNumber: turnsReader.GetInt32(2),
                    Prompt: turnsReader.GetString(3),
                    Response: turnsReader.GetString(4),
                    CreatedAt: DateTimeOffset.Parse(turnsReader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    Citations: citations));
            }
        }

        var result = new List<Conversation>(conversationRows.Count);
        foreach (var row in conversationRows)
        {
            var turns = turnsByConversationId.TryGetValue(row.Id, out var convTurns)
                ? (IReadOnlyList<Turn>)convTurns
                : [];
            result.Add(new Conversation(row.Id, row.Title, row.CreatedAt, row.UpdatedAt, turns));
        }

        return result;
    }

    private async Task EnableForeignKeysAsync(CancellationToken cancellationToken)
    {
        await using var pragmaCmd = _connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        await pragmaCmd.ExecuteNonQueryAsync(cancellationToken);
    }
}

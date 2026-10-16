namespace CopilotChatExporter.Core.Models;

/// <summary>
/// A single continuous dialogue session between a user and Copilot in Microsoft 365 Chat (BizChat).
/// </summary>
public sealed record Conversation(
    string Id,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<Turn> Turns);

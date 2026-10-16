namespace CopilotChatExporter.Core.Models;

/// <summary>
/// A single exchange within a conversation consisting of one user prompt and Copilot's corresponding response.
/// </summary>
public sealed record Turn(
    string Id,
    int SequenceNumber,
    string Prompt,
    string Response,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Citation> Citations);

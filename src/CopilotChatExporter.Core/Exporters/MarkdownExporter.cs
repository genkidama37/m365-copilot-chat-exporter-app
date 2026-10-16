using System.Globalization;
using System.Text;
using CopilotChatExporter.Core.Models;

namespace CopilotChatExporter.Core.Exporters;

internal static class MarkdownExporter
{
    private static readonly HashSet<char> IllegalFileNameCharacters =
    [
        '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\0'
    ];

    public static string BuildSanitizedFileName(DateTimeOffset createdAt, string title, string extension)
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

    public static string FormatConversation(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

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

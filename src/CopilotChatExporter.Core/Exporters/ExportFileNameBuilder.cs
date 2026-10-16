using System.Globalization;
using System.Text;

namespace CopilotChatExporter.Core.Exporters;

internal static class ExportFileNameBuilder
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
}

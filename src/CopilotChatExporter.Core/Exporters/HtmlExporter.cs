using System.Globalization;
using System.Net;
using System.Text;
using CopilotChatExporter.Core.Models;

namespace CopilotChatExporter.Core.Exporters;

internal static class HtmlExporter
{
    private const string SharedBaseStyles = """
                :root {
                  --bg-page: #f8fafc;
                  --bg-surface: #ffffff;
                  --text-primary: #0f172a;
                  --text-muted: #475569;
                  --border-color: #e2e8f0;
                  --prompt-bg: #eff6ff;
                  --prompt-border: #bfdbfe;
                  --response-bg: #ffffff;
                  --response-border: #e2e8f0;
                  --code-bg: #f1f5f9;
                  --badge-bg: #e0f2fe;
                  --badge-text: #0369a1;
                  --link-color: #2563eb;
                }
                @media (prefers-color-scheme: dark) {
                  :root {
                    --bg-page: #0f172a;
                    --bg-surface: #1e293b;
                    --text-primary: #f1f5f9;
                    --text-muted: #94a3b8;
                    --border-color: #334155;
                    --prompt-bg: #1e3a5f;
                    --prompt-border: #2563eb;
                    --response-bg: #1e293b;
                    --response-border: #334155;
                    --code-bg: #0b1120;
                    --badge-bg: #0c4a6e;
                    --badge-text: #7dd3fc;
                    --link-color: #60a5fa;
                  }
                }
                * { box-sizing: border-box; }
                body {
                  margin: 0;
                  padding: 2rem 1rem;
                  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
                  background-color: var(--bg-page);
                  color: var(--text-primary);
                  line-height: 1.6;
                }
                table {
                  width: 100%;
                  border-collapse: collapse;
                  margin: 1rem 0;
                  background-color: var(--bg-surface);
                }
                th, td {
                  border: 1px solid var(--border-color);
                  padding: 0.625rem 0.875rem;
                  text-align: left;
                }
                th {
                  background-color: var(--code-bg);
                  font-weight: 600;
                }
                a {
                  color: var(--link-color);
                }
        """;

    public static string FormatConversation(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        var sb = new StringBuilder();
        var encodedTitle = HtmlEncode(conversation.Title);
        var createdDisplay = conversation.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
        var updatedDisplay = conversation.UpdatedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);

        AppendDocumentHeader(sb, encodedTitle, """
                main {
                  max-width: 900px;
                  margin: 0 auto;
                }
                header.conversation-header {
                  margin-bottom: 2rem;
                  padding-bottom: 1rem;
                  border-bottom: 1px solid var(--border-color);
                }
                header.conversation-header h1 {
                  margin: 0 0 0.5rem 0;
                  font-size: 1.75rem;
                }
                .meta {
                  font-size: 0.875rem;
                  color: var(--text-muted);
                }
                .turn {
                  margin-bottom: 2rem;
                }
                .turn-header {
                  font-size: 0.85rem;
                  font-weight: 600;
                  color: var(--text-muted);
                  margin-bottom: 0.5rem;
                }
                .speech-bubble {
                  padding: 1rem 1.25rem;
                  border-radius: 12px;
                  margin-bottom: 0.75rem;
                  border: 1px solid var(--border-color);
                }
                .prompt-bubble {
                  background-color: var(--prompt-bg);
                  border-color: var(--prompt-border);
                  margin-left: 2rem;
                }
                .response-bubble {
                  background-color: var(--response-bg);
                  border-color: var(--response-border);
                  margin-right: 2rem;
                }
                .bubble-role {
                  font-size: 0.75rem;
                  font-weight: 700;
                  text-transform: uppercase;
                  letter-spacing: 0.05em;
                  color: var(--text-muted);
                  margin-bottom: 0.5rem;
                }
                pre {
                  background-color: var(--code-bg);
                  padding: 1rem;
                  border-radius: 8px;
                  overflow-x: auto;
                  border: 1px solid var(--border-color);
                }
                code {
                  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
                  font-size: 0.9em;
                }
                .citation-badges {
                  margin-top: 0.75rem;
                  display: flex;
                  flex-wrap: wrap;
                  gap: 0.375rem;
                }
                .citation-badge {
                  display: inline-block;
                  padding: 0.15rem 0.5rem;
                  border-radius: 9999px;
                  background-color: var(--badge-bg);
                  color: var(--badge-text);
                  font-size: 0.75rem;
                  font-weight: 600;
                  text-decoration: none;
                }
                section.footnotes {
                  margin-top: 3rem;
                  padding-top: 1.5rem;
                  border-top: 1px solid var(--border-color);
                }
            """);

        sb.Append("    <header class=\"conversation-header\">\n");
        sb.Append($"      <h1>{encodedTitle}</h1>\n");
        sb.Append($"      <div class=\"meta\">Created: {createdDisplay} | Updated: {updatedDisplay} | Turns: {conversation.Turns.Count.ToString(CultureInfo.InvariantCulture)}</div>\n");
        sb.Append("    </header>\n");

        var footnotes = new List<(int Number, Citation Citation)>();
        var nextFootnoteNumber = 1;

        foreach (var turn in conversation.Turns)
        {
            var seq = turn.SequenceNumber.ToString(CultureInfo.InvariantCulture);
            sb.Append($"    <article class=\"turn\" id=\"turn-{seq}\">\n");
            sb.Append($"      <div class=\"turn-header\">Turn {seq}</div>\n");

            sb.Append("      <div class=\"speech-bubble prompt-bubble\">\n");
            sb.Append("        <div class=\"bubble-role\">Prompt</div>\n");
            sb.Append(RenderMarkdownBlocks(turn.Prompt, "        "));
            sb.Append("      </div>\n");

            sb.Append("      <div class=\"speech-bubble response-bubble\">\n");
            sb.Append("        <div class=\"bubble-role\">Response</div>\n");
            sb.Append(RenderMarkdownBlocks(turn.Response, "        "));

            if (turn.Citations.Count > 0)
            {
                sb.Append("        <div class=\"citation-badges\">\n");
                foreach (var citation in turn.Citations)
                {
                    var fnNumber = nextFootnoteNumber++;
                    footnotes.Add((fnNumber, citation));
                    var fnNumStr = fnNumber.ToString(CultureInfo.InvariantCulture);
                    sb.Append($"          <a class=\"citation-badge\" id=\"fnref-{fnNumStr}\" href=\"#fn-{fnNumStr}\" title=\"{HtmlEncode(citation.Title)}\">[{fnNumStr}]</a>\n");
                }

                sb.Append("        </div>\n");
            }

            sb.Append("      </div>\n");
            sb.Append("    </article>\n");
        }

        if (footnotes.Count > 0)
        {
            sb.Append("    <section class=\"footnotes\">\n");
            sb.Append("      <h2>Sources &amp; Citations</h2>\n");
            sb.Append("      <ol>\n");
            foreach (var (number, citation) in footnotes)
            {
                var numStr = number.ToString(CultureInfo.InvariantCulture);
                sb.Append($"        <li id=\"fn-{numStr}\"><a href=\"{HtmlEncode(citation.Url)}\" target=\"_blank\" rel=\"noopener noreferrer\">{HtmlEncode(citation.Title)}</a> <a href=\"#fnref-{numStr}\" aria-label=\"Back to content\">↩</a></li>\n");
            }

            sb.Append("      </ol>\n");
            sb.Append("    </section>\n");
        }

        sb.Append("""
              </main>
            </body>
            </html>
            """.ReplaceLineEndings("\n"));
        sb.Append('\n');

        return sb.ToString();
    }

    public static string FormatMasterIndex(IReadOnlyList<Conversation> conversations)
    {
        ArgumentNullException.ThrowIfNull(conversations);

        var sb = new StringBuilder();
        AppendDocumentHeader(sb, "M365 Copilot Export Catalog", """
                main {
                  max-width: 1000px;
                  margin: 0 auto;
                }
                .search-bar {
                  width: 100%;
                  padding: 0.625rem 0.875rem;
                  margin: 1rem 0 1.5rem 0;
                  border: 1px solid var(--border-color);
                  border-radius: 8px;
                  background-color: var(--bg-surface);
                  color: var(--text-primary);
                  font-size: 1rem;
                }
                th[data-sort-col] {
                  cursor: pointer;
                  user-select: none;
                }
                a {
                  text-decoration: none;
                  margin-right: 0.75rem;
                }
                a:hover {
                  text-decoration: underline;
                }
            """);

        sb.Append("""
                <h1>Exported Conversations Catalog</h1>
                <input id="catalog-search" class="search-bar" type="search" placeholder="Search conversations by title or date..." aria-label="Search conversations" />
                <table id="catalog-table">
                  <thead>
                    <tr>
                      <th data-sort-col="0">Title</th>
                      <th data-sort-col="1">Date</th>
                      <th data-sort-col="2" data-sort-type="number">Turn Count</th>
                      <th>Exports</th>
                    </tr>
                  </thead>
                  <tbody>
            """.ReplaceLineEndings("\n"));
        sb.Append('\n');

        foreach (var conversation in conversations)
        {
            var htmlFileName = ExportFileNameBuilder.BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, ".html");
            var mdFileName = ExportFileNameBuilder.BuildSanitizedFileName(conversation.CreatedAt, conversation.Title, ".md");
            var dateStr = conversation.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var turnCountStr = conversation.Turns.Count.ToString(CultureInfo.InvariantCulture);

            sb.Append("        <tr class=\"catalog-row\">\n");
            sb.Append($"          <td><a href=\"{EncodeRelativeHref(htmlFileName)}\">{HtmlEncode(conversation.Title)}</a></td>\n");
            sb.Append($"          <td>{dateStr}</td>\n");
            sb.Append($"          <td>{turnCountStr}</td>\n");
            sb.Append($"          <td><a href=\"{EncodeRelativeHref(htmlFileName)}\">HTML</a><a href=\"{EncodeRelativeHref(mdFileName)}\">Markdown</a></td>\n");
            sb.Append("        </tr>\n");
        }

        sb.Append("""
                  </tbody>
                </table>
              </main>
              <script>
                (function () {
                  var searchInput = document.getElementById('catalog-search');
                  var tbody = document.querySelector('#catalog-table tbody');
                  var headers = document.querySelectorAll('#catalog-table th[data-sort-col]');
                  if (searchInput && tbody) {
                    searchInput.addEventListener('input', function () {
                      var rows = tbody.querySelectorAll('tr.catalog-row');
                      var query = searchInput.value.toLowerCase().trim();
                      for (var i = 0; i < rows.length; i++) {
                        var text = rows[i].textContent.toLowerCase();
                        rows[i].style.display = text.indexOf(query) !== -1 ? '' : 'none';
                      }
                    });
                  }
                  for (var h = 0; h < headers.length; h++) {
                    headers[h].addEventListener('click', function () {
                      if (!tbody) return;
                      var colIndex = parseInt(this.getAttribute('data-sort-col'), 10);
                      var isNumeric = this.getAttribute('data-sort-type') === 'number';
                      var asc = this.getAttribute('data-sort-dir') !== 'asc';
                      this.setAttribute('data-sort-dir', asc ? 'asc' : 'desc');
                      var rowsArray = Array.prototype.slice.call(tbody.querySelectorAll('tr.catalog-row'));
                      rowsArray.sort(function (a, b) {
                        var aText = a.children[colIndex].textContent.trim();
                        var bText = b.children[colIndex].textContent.trim();
                        var cmp = isNumeric
                          ? (parseFloat(aText) - parseFloat(bText))
                          : aText.localeCompare(bText);
                        return asc ? cmp : -cmp;
                      });
                      for (var r = 0; r < rowsArray.length; r++) {
                        tbody.appendChild(rowsArray[r]);
                      }
                    });
                  }
                })();
              </script>
            </body>
            </html>
            """.ReplaceLineEndings("\n"));
        sb.Append('\n');

        return sb.ToString();
    }

    private static void AppendDocumentHeader(StringBuilder sb, string encodedTitle, string pageSpecificStyles)
    {
        sb.Append("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            """.ReplaceLineEndings("\n"));
        sb.Append('\n');
        sb.Append($"  <title>{encodedTitle}</title>\n");
        sb.Append("  <style>\n");
        sb.Append(SharedBaseStyles.ReplaceLineEndings("\n"));
        sb.Append('\n');
        sb.Append(pageSpecificStyles.ReplaceLineEndings("\n"));
        sb.Append('\n');
        sb.Append("""
              </style>
            </head>
            <body>
              <main>
            """.ReplaceLineEndings("\n"));
        sb.Append('\n');
    }

    private static string RenderMarkdownBlocks(string content, string indent)
    {
        var sb = new StringBuilder();
        var lines = content.ReplaceLineEndings("\n").Split('\n');
        var i = 0;

        while (i < lines.Length)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            var trimmedStart = line.TrimStart();

            // Fenced code block
            if (trimmedStart.StartsWith("```", StringComparison.Ordinal))
            {
                var fenceInfo = trimmedStart[3..].Trim();
                i++;
                var codeBuilder = new StringBuilder();
                while (i < lines.Length && !lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    if (codeBuilder.Length > 0)
                    {
                        codeBuilder.Append('\n');
                    }

                    codeBuilder.Append(lines[i]);
                    i++;
                }

                if (i < lines.Length)
                {
                    i++; // skip closing fence
                }

                var langAttr = string.IsNullOrEmpty(fenceInfo)
                    ? string.Empty
                    : $" class=\"language-{HtmlEncode(fenceInfo)}\"";
                sb.Append($"{indent}<pre><code{langAttr}>{HtmlEncode(codeBuilder.ToString())}</code></pre>\n");
                continue;
            }

            // Markdown table
            if (IsTableStart(lines, i))
            {
                var headers = SplitTableCells(line);
                i += 2; // skip header and separator

                sb.Append($"{indent}<table>\n");
                sb.Append($"{indent}  <thead>\n{indent}    <tr>");
                foreach (var header in headers)
                {
                    sb.Append($"<th>{HtmlEncode(header)}</th>");
                }

                sb.Append($"</tr>\n{indent}  </thead>\n");
                sb.Append($"{indent}  <tbody>\n");

                while (i < lines.Length && IsTableRow(lines[i]))
                {
                    var cells = SplitTableCells(lines[i]);
                    sb.Append($"{indent}    <tr>");
                    foreach (var cell in cells)
                    {
                        sb.Append($"<td>{HtmlEncode(cell)}</td>");
                    }

                    sb.Append("</tr>\n");
                    i++;
                }

                sb.Append($"{indent}  </tbody>\n");
                sb.Append($"{indent}</table>\n");
                continue;
            }

            // Unordered list
            if (IsUnorderedListItem(trimmedStart))
            {
                sb.Append($"{indent}<ul>\n");
                while (i < lines.Length && IsUnorderedListItem(lines[i].TrimStart()))
                {
                    var itemText = lines[i].TrimStart()[2..].Trim();
                    sb.Append($"{indent}  <li>{HtmlEncode(itemText)}</li>\n");
                    i++;
                }

                sb.Append($"{indent}</ul>\n");
                continue;
            }

            // Paragraph (preserving line breaks within contiguous lines)
            var paraBuilder = new StringBuilder();
            while (i < lines.Length &&
                   !string.IsNullOrWhiteSpace(lines[i]) &&
                   !lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal) &&
                   !IsTableStart(lines, i) &&
                   !IsUnorderedListItem(lines[i].TrimStart()))
            {
                if (paraBuilder.Length > 0)
                {
                    paraBuilder.Append("<br />");
                }

                paraBuilder.Append(HtmlEncode(lines[i].Trim()));
                i++;
            }

            sb.Append($"{indent}<p>{paraBuilder}</p>\n");
        }

        return sb.ToString();
    }

    private static bool IsTableStart(string[] lines, int index)
        => IsTableRow(lines[index]) && index + 1 < lines.Length && IsTableSeparatorRow(lines[index + 1]);

    private static bool IsUnorderedListItem(string trimmedLine)
        => trimmedLine.StartsWith("- ", StringComparison.Ordinal) || trimmedLine.StartsWith("* ", StringComparison.Ordinal);

    private static bool IsTableRow(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith('|') && trimmed.EndsWith('|') && trimmed.Length > 2;
    }

    private static bool IsTableSeparatorRow(string line)
    {
        if (!IsTableRow(line))
        {
            return false;
        }

        var cells = SplitTableCells(line);
        return cells.Count > 0 && cells.All(cell => cell.All(ch => ch is '-' or ':' or ' ') && cell.Contains('-'));
    }

    private static List<string> SplitTableCells(string line)
    {
        var trimmed = line.Trim();
        var inner = trimmed[1..^1];
        return inner.Split('|').Select(c => c.Trim()).ToList();
    }

    private static string EncodeRelativeHref(string fileName)
    {
        var uriSafe = fileName
            .Replace("%", "%25", StringComparison.Ordinal)
            .Replace("#", "%23", StringComparison.Ordinal);
        return HtmlEncode(uriSafe);
    }

    private static string HtmlEncode(string value) => WebUtility.HtmlEncode(value);
}

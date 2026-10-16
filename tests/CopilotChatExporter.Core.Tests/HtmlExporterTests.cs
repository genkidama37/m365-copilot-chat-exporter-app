using CopilotChatExporter.Core.Models;
using CopilotChatExporter.Core.Services;
using Microsoft.Data.Sqlite;

namespace CopilotChatExporter.Core.Tests;

public class HtmlExporterTests
{
    [Fact]
    public async Task Exporting_conversation_to_html_produces_standalone_html5_with_speech_bubbles_dark_mode_code_tables_and_citation_footnotes()
    {
        await using var harness = await TestServiceHarness.CreateAsync();

        var createdAt = new DateTimeOffset(2026, 4, 12, 10, 15, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 4, 12, 10, 20, 0, TimeSpan.Zero);

        var responseWithCodeAndTable = """
            Here is the regional latency comparison and configuration snippet:

            | Region | Latency | Status |
            | --- | --- | --- |
            | East US | 22ms | Active |
            | West Europe | 38ms | Standby |

            ```json
            {
              "failover": true,
              "maxRetries": 3
            }
            ```
            """.ReplaceLineEndings("\n");

        var conversation = new Conversation(
            Id: "conv-html-001",
            Title: "Regional Failover: Config & Benchmarks?",
            CreatedAt: createdAt,
            UpdatedAt: updatedAt,
            Turns:
            [
                new Turn(
                    Id: "turn-1",
                    SequenceNumber: 1,
                    Prompt: "Show the latency table and failover config for <East & West>.",
                    Response: responseWithCodeAndTable,
                    CreatedAt: new DateTimeOffset(2026, 4, 12, 10, 16, 0, TimeSpan.Zero),
                    Citations:
                    [
                        new Citation(
                            Id: "cit-1",
                            Title: "Regional Latency Report.xlsx",
                            Url: "https://contoso.sharepoint.com/eng/latency-report.xlsx"),
                        new Citation(
                            Id: "cit-2",
                            Title: "Failover Runbook.docx",
                            Url: "https://contoso.sharepoint.com/eng/failover-runbook.docx")
                    ])
            ]);

        await harness.Service.IngestConversationAsync(conversation);

        var outputDir = Path.Combine(Path.GetTempPath(), $"copilot-html-test-{Guid.NewGuid():N}");
        try
        {
            var htmlFilePath = await harness.Service.ExportConversationToHtmlAsync("conv-html-001", outputDir);

            Assert.Equal("2026-04-12 - Regional Failover Config & Benchmarks.html", Path.GetFileName(htmlFilePath));
            Assert.True(File.Exists(htmlFilePath));

            var html = await File.ReadAllTextAsync(htmlFilePath);

            // Standalone HTML5 structure & embedded CSS with dark mode
            Assert.StartsWith("<!DOCTYPE html>", html.TrimStart(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<style>", html, StringComparison.Ordinal);
            Assert.Contains("@media (prefers-color-scheme: dark)", html, StringComparison.Ordinal);

            // Distinct speech-bubble styling for prompt and response
            Assert.Contains("speech-bubble prompt-bubble", html, StringComparison.Ordinal);
            Assert.Contains("speech-bubble response-bubble", html, StringComparison.Ordinal);

            // HTML escaping in prompt
            Assert.Contains("Show the latency table and failover config for &lt;East &amp; West&gt;.", html, StringComparison.Ordinal);

            // Formatted table and pre-formatted code block
            Assert.Contains("<table>", html, StringComparison.Ordinal);
            Assert.Contains("<th>Region</th>", html, StringComparison.Ordinal);
            Assert.Contains("<td>East US</td>", html, StringComparison.Ordinal);
            Assert.Contains("<td>22ms</td>", html, StringComparison.Ordinal);
            Assert.Contains("<pre><code", html, StringComparison.Ordinal);
            Assert.Contains("&quot;failover&quot;: true", html, StringComparison.Ordinal);

            // Interactive citation footnote badges linking to bottom footnote references
            Assert.Contains("href=\"#fn-1\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"#fn-2\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"fn-1\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"fn-2\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"https://contoso.sharepoint.com/eng/latency-report.xlsx\"", html, StringComparison.Ordinal);
            Assert.Contains("Regional Latency Report.xlsx", html, StringComparison.Ordinal);
            Assert.Contains("href=\"https://contoso.sharepoint.com/eng/failover-runbook.docx\"", html, StringComparison.Ordinal);
            Assert.Contains("Failover Runbook.docx", html, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Exporting_master_index_generates_searchable_offline_catalog_linking_to_html_and_md_exports_with_zero_external_resources()
    {
        await using var harness = await TestServiceHarness.CreateAsync();

        var firstConversation = new Conversation(
            Id: "conv-idx-001",
            Title: "Q1 Security Audit",
            CreatedAt: new DateTimeOffset(2026, 1, 20, 8, 0, 0, TimeSpan.Zero),
            UpdatedAt: new DateTimeOffset(2026, 1, 20, 8, 30, 0, TimeSpan.Zero),
            Turns:
            [
                new Turn(
                    Id: "t-101",
                    SequenceNumber: 1,
                    Prompt: "List the Q1 audit findings.",
                    Response: "All critical findings were remediated.",
                    CreatedAt: new DateTimeOffset(2026, 1, 20, 8, 5, 0, TimeSpan.Zero),
                    Citations:
                    [
                        new Citation(
                            Id: "c-101",
                            Title: "Audit Summary.pdf",
                            Url: "https://contoso.sharepoint.com/sec/audit-summary.pdf")
                    ]),
                new Turn(
                    Id: "t-102",
                    SequenceNumber: 2,
                    Prompt: "Are any low-priority items open?",
                    Response: "Two informational items remain scheduled for Q2.",
                    CreatedAt: new DateTimeOffset(2026, 1, 20, 8, 30, 0, TimeSpan.Zero),
                    Citations: [])
            ]);

        var secondConversation = new Conversation(
            Id: "conv-idx-002",
            Title: "Zero Trust Network Rollout",
            CreatedAt: new DateTimeOffset(2026, 2, 14, 15, 45, 0, TimeSpan.Zero),
            UpdatedAt: new DateTimeOffset(2026, 2, 14, 16, 0, 0, TimeSpan.Zero),
            Turns:
            [
                new Turn(
                    Id: "t-201",
                    SequenceNumber: 1,
                    Prompt: "What is the rollout schedule?",
                    Response: "Phase 1 completes in March.",
                    CreatedAt: new DateTimeOffset(2026, 2, 14, 15, 50, 0, TimeSpan.Zero),
                    Citations: [])
            ]);

        await harness.Service.IngestConversationAsync(firstConversation);
        await harness.Service.IngestConversationAsync(secondConversation);

        var outputDir = Path.Combine(Path.GetTempPath(), $"copilot-index-test-{Guid.NewGuid():N}");
        try
        {
            var htmlPath1 = await harness.Service.ExportConversationToHtmlAsync("conv-idx-001", outputDir);
            var mdPath1 = await harness.Service.ExportConversationToMarkdownAsync("conv-idx-001", outputDir);
            var htmlPath2 = await harness.Service.ExportConversationToHtmlAsync("conv-idx-002", outputDir);
            var mdPath2 = await harness.Service.ExportConversationToMarkdownAsync("conv-idx-002", outputDir);

            var indexPath = await harness.Service.ExportMasterIndexAsync(outputDir);

            Assert.Equal("index.html", Path.GetFileName(indexPath));
            Assert.True(File.Exists(indexPath));

            var indexHtml = await File.ReadAllTextAsync(indexPath);

            // Standalone HTML5 + dark mode
            Assert.StartsWith("<!DOCTYPE html>", indexHtml.TrimStart(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<style>", indexHtml, StringComparison.Ordinal);
            Assert.Contains("@media (prefers-color-scheme: dark)", indexHtml, StringComparison.Ordinal);

            // Summary table with Title, Date, Turn Count
            Assert.Contains("<table", indexHtml, StringComparison.Ordinal);
            Assert.Contains("Title", indexHtml, StringComparison.Ordinal);
            Assert.Contains("Date", indexHtml, StringComparison.Ordinal);
            Assert.Contains("Turn Count", indexHtml, StringComparison.Ordinal);

            Assert.Contains("Q1 Security Audit", indexHtml, StringComparison.Ordinal);
            Assert.Contains("2026-01-20", indexHtml, StringComparison.Ordinal);
            Assert.Contains(">2<", indexHtml, StringComparison.Ordinal);

            Assert.Contains("Zero Trust Network Rollout", indexHtml, StringComparison.Ordinal);
            Assert.Contains("2026-02-14", indexHtml, StringComparison.Ordinal);
            Assert.Contains(">1<", indexHtml, StringComparison.Ordinal);

            // Working relative links to both .html and .md files
            Assert.Contains($"href=\"{Path.GetFileName(htmlPath1)}\"", indexHtml, StringComparison.Ordinal);
            Assert.Contains($"href=\"{Path.GetFileName(mdPath1)}\"", indexHtml, StringComparison.Ordinal);
            Assert.Contains($"href=\"{Path.GetFileName(htmlPath2)}\"", indexHtml, StringComparison.Ordinal);
            Assert.Contains($"href=\"{Path.GetFileName(mdPath2)}\"", indexHtml, StringComparison.Ordinal);

            // Embedded offline client-side search/filter JavaScript
            Assert.Contains("<input", indexHtml, StringComparison.Ordinal);
            Assert.Contains("<script>", indexHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("<script src=", indexHtml, StringComparison.OrdinalIgnoreCase);

            // Air-gapped offline verification on both conversation HTML and master index HTML
            var conversationHtml1 = await File.ReadAllTextAsync(htmlPath1);
            AssertAirGappedHtml(conversationHtml1, allowedCitationUrls: ["https://contoso.sharepoint.com/sec/audit-summary.pdf"]);
            AssertAirGappedHtml(indexHtml, allowedCitationUrls: []);
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    private static void AssertAirGappedHtml(string html, IReadOnlyList<string> allowedCitationUrls)
    {
        Assert.DoesNotContain("<link", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("@import", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script src", html, StringComparison.OrdinalIgnoreCase);

        var sanitized = html;
        foreach (var url in allowedCitationUrls)
        {
            sanitized = sanitized.Replace($"href=\"{url}\"", string.Empty, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("http://", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestServiceHarness : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestServiceHarness(SqliteConnection connection, ICopilotExportService service)
        {
            _connection = connection;
            Service = service;
        }

        public ICopilotExportService Service { get; }

        public static async Task<TestServiceHarness> CreateAsync()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            await connection.OpenAsync();
            var service = new CopilotExportService(connection);
            await service.InitializeAsync();
            return new TestServiceHarness(connection, service);
        }

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }
}

using CopilotChatExporter.Core.Models;
using CopilotChatExporter.Core.Services;
using Microsoft.Data.Sqlite;

namespace CopilotChatExporter.Core.Tests;

public class CopilotExportServiceTests
{
    [Fact]
    public async Task Ingesting_conversation_with_turns_and_citations_persists_with_exact_relational_integrity_and_timestamps()
    {
        await using var harness = await TestServiceHarness.CreateAsync();

        var createdAt = new DateTimeOffset(2026, 3, 15, 9, 30, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 3, 15, 9, 35, 42, TimeSpan.Zero);
        var turn1Timestamp = new DateTimeOffset(2026, 3, 15, 9, 30, 10, TimeSpan.Zero);
        var turn2Timestamp = new DateTimeOffset(2026, 3, 15, 9, 35, 42, TimeSpan.Zero);

        var conversation = new Conversation(
            Id: "conv-001",
            Title: "Q3 Architecture Review",
            CreatedAt: createdAt,
            UpdatedAt: updatedAt,
            Turns:
            [
                new Turn(
                    Id: "turn-001",
                    SequenceNumber: 1,
                    Prompt: "Summarize the Q3 architecture proposals.",
                    Response: "The Q3 proposals focus on regional failover and zero-trust gateways.",
                    CreatedAt: turn1Timestamp,
                    Citations:
                    [
                        new Citation(
                            Id: "cit-001",
                            Title: "Q3 Architecture RFC.docx",
                            Url: "https://contoso.sharepoint.com/docs/q3-rfc.docx"),
                        new Citation(
                            Id: "cit-002",
                            Title: "Security Baseline 2026.pdf",
                            Url: "https://contoso.sharepoint.com/docs/security-2026.pdf")
                    ]),
                new Turn(
                    Id: "turn-002",
                    SequenceNumber: 2,
                    Prompt: "What are the latency targets?",
                    Response: "The target p99 latency across regions is under 45ms.",
                    CreatedAt: turn2Timestamp,
                    Citations:
                    [
                        new Citation(
                            Id: "cit-003",
                            Title: "SLA Targets Spreadsheet.xlsx",
                            Url: "https://contoso.sharepoint.com/docs/sla-targets.xlsx")
                    ])
            ]);

        await harness.Service.IngestConversationAsync(conversation);

        var persisted = await harness.Service.GetConversationAsync("conv-001");

        Assert.NotNull(persisted);
        Assert.Equal("conv-001", persisted.Id);
        Assert.Equal("Q3 Architecture Review", persisted.Title);
        Assert.Equal(createdAt, persisted.CreatedAt);
        Assert.Equal(updatedAt, persisted.UpdatedAt);
        Assert.Equal(2, persisted.Turns.Count);

        var firstTurn = persisted.Turns[0];
        Assert.Equal("turn-001", firstTurn.Id);
        Assert.Equal(1, firstTurn.SequenceNumber);
        Assert.Equal("Summarize the Q3 architecture proposals.", firstTurn.Prompt);
        Assert.Equal("The Q3 proposals focus on regional failover and zero-trust gateways.", firstTurn.Response);
        Assert.Equal(turn1Timestamp, firstTurn.CreatedAt);
        Assert.Equal(2, firstTurn.Citations.Count);
        Assert.Equal("cit-001", firstTurn.Citations[0].Id);
        Assert.Equal("Q3 Architecture RFC.docx", firstTurn.Citations[0].Title);
        Assert.Equal("https://contoso.sharepoint.com/docs/q3-rfc.docx", firstTurn.Citations[0].Url);
        Assert.Equal("cit-002", firstTurn.Citations[1].Id);
        Assert.Equal("Security Baseline 2026.pdf", firstTurn.Citations[1].Title);
        Assert.Equal("https://contoso.sharepoint.com/docs/security-2026.pdf", firstTurn.Citations[1].Url);

        var secondTurn = persisted.Turns[1];
        Assert.Equal("turn-002", secondTurn.Id);
        Assert.Equal(2, secondTurn.SequenceNumber);
        Assert.Equal("What are the latency targets?", secondTurn.Prompt);
        Assert.Equal("The target p99 latency across regions is under 45ms.", secondTurn.Response);
        Assert.Equal(turn2Timestamp, secondTurn.CreatedAt);
        Assert.Single(secondTurn.Citations);
        Assert.Equal("cit-003", secondTurn.Citations[0].Id);
        Assert.Equal("SLA Targets Spreadsheet.xlsx", secondTurn.Citations[0].Title);
        Assert.Equal("https://contoso.sharepoint.com/docs/sla-targets.xlsx", secondTurn.Citations[0].Url);
    }

    [Fact]
    public async Task Exporting_ingested_conversation_to_markdown_writes_sanitized_file_with_frontmatter_turns_and_footnote_citations()
    {
        await using var harness = await TestServiceHarness.CreateAsync();

        var createdAt = new DateTimeOffset(2026, 4, 10, 14, 0, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 4, 10, 14, 12, 30, TimeSpan.Zero);

        var conversation = new Conversation(
            Id: "conv-2026-042",
            Title: "Q3/Q4: Budget & Roadmap? <Draft*>",
            CreatedAt: createdAt,
            UpdatedAt: updatedAt,
            Turns:
            [
                new Turn(
                    Id: "turn-1",
                    SequenceNumber: 1,
                    Prompt: "What are the key Q3 budget priorities?",
                    Response: "The top priorities are cloud cost optimization and regional redundancy.",
                    CreatedAt: new DateTimeOffset(2026, 4, 10, 14, 1, 0, TimeSpan.Zero),
                    Citations:
                    [
                        new Citation(
                            Id: "cit-1",
                            Title: "FY26 Cloud Budget.xlsx",
                            Url: "https://contoso.sharepoint.com/finance/fy26-cloud-budget.xlsx"),
                        new Citation(
                            Id: "cit-2",
                            Title: "Resilience Strategy.docx",
                            Url: "https://contoso.sharepoint.com/eng/resilience-strategy.docx")
                    ]),
                new Turn(
                    Id: "turn-2",
                    SequenceNumber: 2,
                    Prompt: "How much is allocated to redundancy?",
                    Response: "$1.2M is allocated across two failover regions.",
                    CreatedAt: new DateTimeOffset(2026, 4, 10, 14, 12, 30, TimeSpan.Zero),
                    Citations:
                    [
                        new Citation(
                            Id: "cit-3",
                            Title: "Failover Cost Breakdown.pdf",
                            Url: "https://contoso.sharepoint.com/finance/failover-costs.pdf")
                    ])
            ]);

        await harness.Service.IngestConversationAsync(conversation);

        var outputDir = Path.Combine(Path.GetTempPath(), $"copilot-export-test-{Guid.NewGuid():N}");
        try
        {
            var filePath = await harness.Service.ExportConversationToMarkdownAsync("conv-2026-042", outputDir);

            Assert.Equal("2026-04-10 - Q3Q4 Budget & Roadmap Draft.md", Path.GetFileName(filePath));
            Assert.True(File.Exists(filePath));

            var markdown = await File.ReadAllTextAsync(filePath);
            var expectedMarkdown = """
                ---
                id: "conv-2026-042"
                title: "Q3/Q4: Budget & Roadmap? <Draft*>"
                created_at: "2026-04-10T14:00:00.0000000+00:00"
                updated_at: "2026-04-10T14:12:30.0000000+00:00"
                turns: 2
                ---

                # Q3/Q4: Budget & Roadmap? <Draft*>

                ## Turn 1

                ### Prompt

                What are the key Q3 budget priorities?

                ### Response

                The top priorities are cloud cost optimization and regional redundancy. [^1] [^2]

                ## Turn 2

                ### Prompt

                How much is allocated to redundancy?

                ### Response

                $1.2M is allocated across two failover regions. [^3]

                ## Sources & Citations

                [^1]: [FY26 Cloud Budget.xlsx](https://contoso.sharepoint.com/finance/fy26-cloud-budget.xlsx)
                [^2]: [Resilience Strategy.docx](https://contoso.sharepoint.com/eng/resilience-strategy.docx)
                [^3]: [Failover Cost Breakdown.pdf](https://contoso.sharepoint.com/finance/failover-costs.pdf)
                """.ReplaceLineEndings("\n") + "\n";

            Assert.Equal(expectedMarkdown, markdown.ReplaceLineEndings("\n"));
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }
}

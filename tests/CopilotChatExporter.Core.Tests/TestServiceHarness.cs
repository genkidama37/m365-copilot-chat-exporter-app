using CopilotChatExporter.Core.Services;
using Microsoft.Data.Sqlite;

namespace CopilotChatExporter.Core.Tests;

internal sealed class TestServiceHarness : IAsyncDisposable
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

using System.Text.Json;
using StackExchange.Redis;
using WRS.Domain.Infrastructure;
using WRS.Domain.Types;

namespace WRS.Infrastructure.Valkey;

public class RequestRepository : IRequestRepository
{
    private ITransaction Transaction { get; }
    private ValkeyOptions Options { get; }

    public RequestRepository(ITransaction transaction, ValkeyOptions options)
    {
        Transaction = transaction;
        Options = options;
    }

    public async Task PersistRequestAsync(Request request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestId = Guid.NewGuid().ToString("N");
        var requestKey = $"{Options.RequestKeyPrefix}:{requestId}";
        var requestIndexKey = $"{Options.RequestKeyPrefix}:index";
        var payload = JsonSerializer.Serialize(new StoredRequest(
            requestId,
            DateTimeOffset.UtcNow,
            request.Method,
            request.Path,
            request.RoutePath,
            request.Query,
            request.Body));

        var storeRequestTask = Transaction.StringSetAsync(requestKey, payload);
        var appendToIndexTask = Transaction.ListRightPushAsync(requestIndexKey, requestId);

        var committed = await Transaction.ExecuteAsync();
        if (!committed)
        {
            throw new InvalidOperationException("Valkey transaction was not committed.");
        }

        await Task.WhenAll(storeRequestTask, appendToIndexTask);
    }

    private sealed record StoredRequest(
        string Id,
        DateTimeOffset CreatedAtUtc,
        string Method,
        string Path,
        string? RoutePath,
        IReadOnlyDictionary<string, string> Query,
        string Body);
}
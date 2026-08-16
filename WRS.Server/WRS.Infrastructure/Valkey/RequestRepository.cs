using System.Text.Json;
using WRS.Domain.Infrastructure;
using WRS.Domain.Types;

namespace WRS.Infrastructure.Valkey;

public class RequestRepository : IRequestRepository
{
    private const string RequestKeyPrefix = "requests";
    
    private ValkeyTransactionScope TransactionScope { get; }

    public RequestRepository(ValkeyTransactionScope transactionScope)
    {
        TransactionScope = transactionScope;
    }

    public Task PersistRequestAsync(Request request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestId = Guid.NewGuid().ToString("N");
        var requestKey = $"{RequestKeyPrefix}:{requestId}";
        var requestIndexKey = $"{RequestKeyPrefix}:index";
        var payload = JsonSerializer.Serialize(new StoredRequest(
            requestId,
            DateTimeOffset.UtcNow,
            request.Method,
            request.Path,
            request.RoutePath,
            request.Query,
            request.Body));

        TransactionScope.Enqueue(
            TransactionScope.Transaction.StringSetAsync(requestKey, payload),
            TransactionScope.Transaction.ListRightPushAsync(requestIndexKey, requestId));

        return Task.CompletedTask;
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
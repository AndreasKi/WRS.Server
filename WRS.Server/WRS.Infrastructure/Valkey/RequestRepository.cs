using System.Text.Json;
using JetBrains.Annotations;
using StackExchange.Redis;
using WRS.Domain.Infrastructure;
using WRS.Domain.Types;

namespace WRS.Infrastructure.Valkey;

[UsedImplicitly]
public class RequestRepository : IRequestRepository
{
    private const string RequestKeyPrefix = "requests";
    private const string RequestIndexKey = "requests:index";

    private IConnectionMultiplexer ConnectionMultiplexer { get; }
    private ValkeyTransactionScope TransactionScope { get; }

    public RequestRepository(IConnectionMultiplexer connectionMultiplexer, ValkeyTransactionScope transactionScope)
    {
        ConnectionMultiplexer = connectionMultiplexer;
        TransactionScope = transactionScope;
    }

    public Task PersistRequestAsync(Request request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestId = Guid.NewGuid().ToString("N");
        var payload = JsonSerializer.Serialize(new PersistedRequest
        {
            Id = requestId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Method = request.Method,
            Path = request.Path,
            RoutePath = request.RoutePath,
            Query = request.Query,
            Body = request.Body
        });

        TransactionScope.Enqueue(
            TransactionScope.Transaction.StringSetAsync(GetRequestKey(requestId), payload),
            TransactionScope.Transaction.ListRightPushAsync(RequestIndexKey, requestId));

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<PersistedRequest>> GetPersistedRequestsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var database = ConnectionMultiplexer.GetDatabase();
        var requestIds = await database.ListRangeAsync(RequestIndexKey);
        if (requestIds.Length == 0)
        {
            return [];
        }

        var requestKeys = requestIds
            .Select(requestId => (RedisKey)GetRequestKey(requestId.ToString()))
            .ToArray();
        var payloads = await database.StringGetAsync(requestKeys);
        var persistedRequests = new PersistedRequest[payloads.Length];

        for (var index = 0; index < payloads.Length; index++)
        {
            var payload = payloads[index];
            if (payload.IsNullOrEmpty)
            {
                throw new InvalidOperationException($"Persisted request '{requestIds[index]}' could not be found.");
            }

            persistedRequests[index] = JsonSerializer.Deserialize<PersistedRequest>(payload.ToString())
                ?? throw new InvalidOperationException($"Persisted request '{requestIds[index]}' could not be deserialized.");
        }

        return persistedRequests;
    }

    private static string GetRequestKey(string requestId) => $"{RequestKeyPrefix}:{requestId}";
}
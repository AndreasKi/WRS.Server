using StackExchange.Redis;

namespace WRS.Infrastructure.Valkey;

public sealed class ValkeyTransactionScope
{
    private IConnectionMultiplexer ConnectionMultiplexer { get; }
    private List<Task> PendingOperations { get; } = [];
    private ITransaction? TransactionInstance { get; set; }
    private bool IsCommitted { get; set; }

    public ITransaction Transaction => TransactionInstance ??= ConnectionMultiplexer.GetDatabase().CreateTransaction();

    public ValkeyTransactionScope(IConnectionMultiplexer connectionMultiplexer)
    {
        ConnectionMultiplexer = connectionMultiplexer;
    }

    public void Enqueue(params Task[] operations)
    {
        if (IsCommitted)
        {
            throw new InvalidOperationException("Valkey transaction has already been committed.");
        }

        PendingOperations.AddRange(operations);
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsCommitted)
        {
            throw new InvalidOperationException("Valkey transaction has already been committed.");
        }

        if (PendingOperations.Count == 0)
        {
            return;
        }

        IsCommitted = true;

        var committed = await Transaction.ExecuteAsync();
        if (!committed)
        {
            throw new InvalidOperationException("Valkey transaction was not committed.");
        }

        await Task.WhenAll(PendingOperations);
    }
}

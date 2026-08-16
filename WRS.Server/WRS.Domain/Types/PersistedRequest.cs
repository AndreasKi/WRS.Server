namespace WRS.Domain.Types;

public sealed class PersistedRequest : Request
{
    public required string Id { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
}

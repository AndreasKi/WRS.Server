namespace WRS.Infrastructure.Valkey;

public sealed class ValkeyOptions
{
    public const string SectionName = "Valkey";

    public required string Configuration { get; init; }
}

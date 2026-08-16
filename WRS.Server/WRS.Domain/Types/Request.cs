namespace WRS.Domain.Types;

public class Request
{
    public required string Method { get; init; }
    
    public required string Path { get; init; }
    
    public required string? RoutePath { get; init; }
    
    public required Dictionary<string, string> Query { get; init; }
    
    public required string Body { get; init; }
}
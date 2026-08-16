using WRS.Domain;
using WRS.Domain.Requests;
using WRS.Domain.Types;
using WRS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddInfrastructure()
    .AddDomain();

var app = builder.Build();

app.MapOpenApi();
app.Map("/{**path}", async (HttpContext context, string? path, IRequestService service, CancellationToken ct) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync(ct);
    
    await service.PersistRequestAsync(
        new Request
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            RoutePath = path,
            Query = context.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Body = body
        }, 
        ct);
    
    return Results.Ok();
});

app.Run();
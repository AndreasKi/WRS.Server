using WRS.Domain;
using WRS.Domain.Requests;
using WRS.Domain.Types;
using WRS.Infrastructure;
using WRS.Infrastructure.Valkey;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddOpenApi()
    .AddInfrastructure(builder.Configuration)
    .AddDomain();

var app = builder.Build();

app.MapOpenApi();
app.MapControllers();
app.Use(async (context, next) =>
{
    await next();
    await context.RequestServices
        .GetRequiredService<ValkeyTransactionScope>()
        .CommitAsync(context.RequestAborted);
});
app.Map("/hook/{**path}", async (HttpContext context, string? path, IRequestService service, CancellationToken ct) =>
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
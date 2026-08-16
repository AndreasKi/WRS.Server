using Microsoft.AspNetCore.Mvc;
using WRS.Domain.Requests;

namespace WRS.Server.Controllers;

[ApiController]
[Route("requests")]
public sealed class PersistedRequestsController : ControllerBase
{
    private IRequestService RequestService { get; }

    public PersistedRequestsController(IRequestService requestService)
    {
        RequestService = requestService;
    }

    [HttpGet]
    [ProducesResponseType<PersistedRequestResponse[]>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersistedRequestResponse>>> GetPersistedRequestsAsync(CancellationToken cancellationToken)
    {
        var requests = await RequestService.GetPersistedRequestsAsync(cancellationToken);

        return Ok(requests
            .Select(request => new PersistedRequestResponse(
                request.Id,
                request.CreatedAtUtc,
                request.Method,
                request.Path,
                request.RoutePath,
                request.Query,
                request.Body))
            .ToArray());
    }

    public sealed record PersistedRequestResponse(
        string Id,
        DateTimeOffset CreatedAtUtc,
        string Method,
        string Path,
        string? RoutePath,
        IReadOnlyDictionary<string, string> Query,
        string Body);
}

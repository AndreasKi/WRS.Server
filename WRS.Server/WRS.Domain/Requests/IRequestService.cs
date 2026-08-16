using WRS.Domain.Types;

namespace WRS.Domain.Requests;

public interface IRequestService
{
    Task PersistRequestAsync(Request request, CancellationToken cancellationToken);

    Task<IReadOnlyList<PersistedRequest>> GetPersistedRequestsAsync(CancellationToken cancellationToken);
}
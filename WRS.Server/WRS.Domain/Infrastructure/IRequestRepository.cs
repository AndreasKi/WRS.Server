using WRS.Domain.Types;

namespace WRS.Domain.Infrastructure;

public interface IRequestRepository
{
    public Task<Guid> PersistRequestAsync(Request request, CancellationToken cancellationToken);

    public Task<IReadOnlyList<PersistedRequest>> GetPersistedRequestsAsync(CancellationToken cancellationToken);
}
using WRS.Domain.Types;

namespace WRS.Domain.Infrastructure;

public interface IRequestRepository
{
    public Task PersistRequestAsync(Request request, CancellationToken cancellationToken);
}
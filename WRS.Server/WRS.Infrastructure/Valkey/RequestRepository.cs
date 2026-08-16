using WRS.Domain.Infrastructure;
using WRS.Domain.Types;

namespace WRS.Infrastructure.Valkey;

public class RequestRepository : IRequestRepository
{
    public Task PersistRequestAsync(Request request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
using WRS.Domain.Infrastructure;
using WRS.Domain.Types;

namespace WRS.Domain.Requests;

public class RequestService : IRequestService
{
    private IRequestRepository _requestRepository { get; }
    
    public RequestService(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public Task PersistRequestAsync(Request request, CancellationToken cancellationToken)
    {
        return _requestRepository.PersistRequestAsync(request, cancellationToken);
    }
}
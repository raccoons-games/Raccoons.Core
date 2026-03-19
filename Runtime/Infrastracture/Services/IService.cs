using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Infrastructure
{
    public interface IService
    {
        int InitOrder => 0;
        bool Awaitable => false;

        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}

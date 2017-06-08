using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IMyStatefulService : IService
    {
        Task AddToQueueAsync(OptOut request);
    }
}
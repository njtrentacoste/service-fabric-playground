using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IOptOutRepository
    {
        Task<List<OptOut>> GetOptOutsAsync();
        Task AddOptOutAsync(OptOut request);
    }
}
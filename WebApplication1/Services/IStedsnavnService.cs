using WebApplication1.API_Models;

namespace WebApplication1.Services
{
    public interface IStedsnavnService
    {
        Task<StedsnavnResponse> GetStedsnavnAsync(string search);
    }
}
using KamPay.Models;

namespace KamPay.Services;
// ===== S�RPR�Z KUTU SERV�S� =====
public interface ISurpriseBoxService
{
    Task<ServiceResult<SurpriseBox>> CreateSurpriseBoxAsync(string productId, User donor);
    Task<ServiceResult<SurpriseBox>> OpenRandomBoxAsync(string userId);
    Task<ServiceResult<List<SurpriseBox>>> GetAvailableBoxesAsync();
}
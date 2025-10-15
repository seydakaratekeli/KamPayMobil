using System.Threading.Tasks;
using KamPay.Models;

namespace KamPay.Services
{
    public interface IAuthenticationService
    {
        /// Yeni kullan�c� kayd� yapar ve do�rulama kodu g�nderir
        Task<ServiceResult<User>> RegisterAsync(RegisterRequest request);

        /// Kullan�c� giri�i yapar
        Task<ServiceResult<User>> LoginAsync(LoginRequest request);

        /// E-posta do�rulama kodu g�nderir
        Task<ServiceResult<bool>> SendVerificationCodeAsync(string email);

        /// E-posta do�rulama kodunu kontrol eder
        Task<ServiceResult<bool>> VerifyEmailAsync(VerificationRequest request);

        /// Kay�t iste�ini do�rular
        ValidationResult ValidateRegistration(RegisterRequest request);

        /// Giri� iste�ini do�rular
        ValidationResult ValidateLogin(LoginRequest request);

        /// Kullan�c� ��k��� yapar
        Task<ServiceResult<bool>> LogoutAsync();

        /// �u anki kullan�c�y� getirir
        Task<User> GetCurrentUserAsync();

        /// Kullan�c�n�n giri� yap�p yapmad���n� kontrol eder
        bool IsUserLoggedIn();
    }
}
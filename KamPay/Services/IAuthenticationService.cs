using System.Threading.Tasks;
using KamPay.Models;

namespace KamPay.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Yeni kullan�c� kayd� yapar ve do�rulama kodu g�nderir
        /// </summary>
        Task<ServiceResult<User>> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Kullan�c� giri�i yapar
        /// </summary>
        Task<ServiceResult<User>> LoginAsync(LoginRequest request);

        /// <summary>
        /// E-posta do�rulama kodu g�nderir
        /// </summary>
        Task<ServiceResult<bool>> SendVerificationCodeAsync(string email);

        /// <summary>
        /// E-posta do�rulama kodunu kontrol eder
        /// </summary>
        Task<ServiceResult<bool>> VerifyEmailAsync(VerificationRequest request);

        /// <summary>
        /// Kay�t iste�ini do�rular
        /// </summary>
        ValidationResult ValidateRegistration(RegisterRequest request);

        /// <summary>
        /// Giri� iste�ini do�rular
        /// </summary>
        ValidationResult ValidateLogin(LoginRequest request);

        /// <summary>
        /// Kullan�c� ��k��� yapar
        /// </summary>
        Task<ServiceResult<bool>> LogoutAsync();

        /// <summary>
        /// �u anki kullan�c�y� getirir
        /// </summary>
        Task<User> GetCurrentUserAsync();

        /// <summary>
        /// Kullan�c�n�n giri� yap�p yapmad���n� kontrol eder
        /// </summary>
        bool IsUserLoggedIn();
    }
}
using System.Threading.Tasks;

namespace KamPay.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Do�rulama e-postas� g�nderir. True d�nerse g�nderim ba�ar�l�d�r.
        /// </summary>
        Task<bool> SendVerificationEmailAsync(string toEmail, string verificationCode);
    }
}

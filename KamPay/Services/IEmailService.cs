using System.Threading.Tasks;

namespace KamPay.Services
{
    public interface IEmailService
    {
        /// Do�rulama e-postas� g�nderir. True d�nerse g�nderim ba�ar�l�d�r.
        Task<bool> SendVerificationEmailAsync(string toEmail, string verificationCode);
    }
}

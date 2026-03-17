using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}

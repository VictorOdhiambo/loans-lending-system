using System.Threading.Tasks;

namespace LoanApplicationService.Web.Helpers
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}

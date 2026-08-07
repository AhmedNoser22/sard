namespace Sard.Infrastructure.Jobs
{
    public class EmailQueueJob(IEmailService emailService)
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            await emailService.SendEmailAsync(to, subject, body);
        }
    }
}

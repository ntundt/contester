using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace contester.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body);
}

public class EmailService(IConfiguration configurationOld, IConfigurationReaderService configuration) : IEmailService
{
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SQL Contest", configuration.GetEmailUser()));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };
        
        using var client = new SmtpClient();

        if (configuration.GetEmailDoNotCheckCertificateRevocation())
        {
            client.CheckCertificateRevocation = false;
        }
        
        if (configuration.GetEmailUseStartTls())
        {
            await client.ConnectAsync(configuration.GetEmailHost(), configuration.GetEmailPort(), SecureSocketOptions.StartTls);
        }
        else
        {
            await client.ConnectAsync(configuration.GetEmailHost(), configuration.GetEmailPort(), true);
        }
        await client.AuthenticateAsync(configuration.GetEmailUser(), configuration.GetEmailPassword());
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
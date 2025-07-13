using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace contester.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body);
}

public class EmailService(IConfiguration configuration) : IEmailService
{
    private bool CheckParameters()
    {
        return configuration["Email:Host"] != null &&
               configuration["Email:Port"] != null &&
               configuration["Email:Username"] != null &&
               configuration["Email:Password"] != null;
    }
    
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        if (!CheckParameters())
        {
            throw new ApplicationException("Email service parameters are not provided");
        }
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SQL Contest", configuration["Email:Username"]));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };
        
        using var client = new SmtpClient();

        if (bool.TryParse(configuration["Email:DoNotCheckCertificateRevocation"], out var trustCertificate) && trustCertificate)
        {
            client.CheckCertificateRevocation = false;
        }
        
        if (bool.TryParse(configuration["Email:UseStartTls"], out var useStartTls) && useStartTls)
        {
            await client.ConnectAsync(configuration["Email:Host"], int.Parse(configuration["Email:Port"]!), SecureSocketOptions.StartTls);
        }
        else
        {
            await client.ConnectAsync(configuration["Email:Host"], int.Parse(configuration["Email:Port"]!), true);
        }
        await client.AuthenticateAsync(configuration["Email:Username"], configuration["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace diploma.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private bool CheckParameters()
    {
        return _configuration["Email:Host"] != null &&
               _configuration["Email:Port"] != null &&
               _configuration["Email:Username"] != null &&
               _configuration["Email:Password"] != null;
    }
    
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        if (!CheckParameters())
        {
            throw new ApplicationException("Email service parameters are not provided");
        }
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SQL Contest", _configuration["Email:Username"]));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };
        
        using var client = new SmtpClient();

        if (bool.TryParse(_configuration["Email:DoNotCheckCertificateRevocation"], out var trustCertificate) && trustCertificate)
        {
            client.CheckCertificateRevocation = false;
        }
        
        if (bool.TryParse(_configuration["Email:UseStartTls"], out var useStartTls) && useStartTls)
        {
            await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"]!), SecureSocketOptions.StartTls);
        }
        else
        {
            await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"]!), true);
        }
        await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
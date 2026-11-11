using FunkoApiV._2.Config;
using FunkoApiV._2.Infrastructure.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace FunkoApiV._2.Service.Email;

public class EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> log) : IEmailMessage, IScopedService {
    private readonly SmtpSettings _settings = options.Value;
    
    public async Task SendEmailAsync(EmailMessage message) {
        var mimeMessage = new MimeMessage {
            Subject = message.Subject,
            Body = new TextPart(message.IsHtml ? TextFormat.Html : TextFormat.Plain) {
                Text = message.Body
            }
        };
        mimeMessage.From.Add(new MailboxAddress(_settings.DisplayName,_settings.From));
        mimeMessage.To.Add(new MailboxAddress("", message.To));
        try {
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(_settings.Username)) {
                await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password);
            }
            await smtpClient.SendAsync(mimeMessage);
            await smtpClient.DisconnectAsync(true);
        } catch (Exception e) {
            log.LogError(e, "Error while sending email");
            throw;
        }
    }
}
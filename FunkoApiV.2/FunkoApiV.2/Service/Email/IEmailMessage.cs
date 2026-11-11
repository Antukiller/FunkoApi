namespace FunkoApiV._2.Service.Email;

public interface IEmailMessage {
    Task SendEmailAsync(EmailMessage message);
}

public class EmailMessage {
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body {  get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}
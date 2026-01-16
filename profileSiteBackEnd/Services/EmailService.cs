using System.Net;
using System.Net.Mail;

namespace profileSiteBackEnd.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendContactEmailAsync(string name, string email, string? subject, string message)
        {
            try
            {
                var smtpHost = _config["Email:SmtpHost"];
                var smtpPort = _config.GetValue<int>("Email:SmtpPort", 587);
                var smtpUser = _config["Email:SmtpUser"];
                var smtpPass = _config["Email:SmtpPassword"];
                var fromEmail = _config["Email:FromEmail"];
                var toEmail = _config["Email:ToEmail"] ?? fromEmail;

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || 
                    string.IsNullOrEmpty(smtpPass) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("Email configuration is incomplete");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var emailSubject = string.IsNullOrEmpty(subject) 
                    ? $"Portfolio Contact from {name}"
                    : $"Portfolio Contact: {subject}";

                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f4f4f4; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ background-color: #fff; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .field {{ margin-bottom: 15px; }}
        .label {{ font-weight: bold; color: #555; }}
        .message {{ background-color: #f9f9f9; padding: 15px; border-left: 4px solid #007bff; margin: 15px 0; }}
        .footer {{ margin-top: 20px; padding-top: 15px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>New Portfolio Contact Form Submission</h2>
        </div>
        <div class='content'>
            <div class='field'>
                <div class='label'>Name:</div>
                <div>{name}</div>
            </div>
            <div class='field'>
                <div class='label'>Email:</div>
                <div>{email}</div>
            </div>
            <div class='field'>
                <div class='label'>Subject:</div>
                <div>{subject ?? "No subject provided"}</div>
            </div>
            <div class='field'>
                <div class='label'>Message:</div>
                <div class='message'>{message.Replace("\n", "<br>")}</div>
            </div>
        </div>
        <div class='footer'>
            <p>Sent on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC from your portfolio contact form</p>
        </div>
    </div>
</body>
</html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Portfolio Contact Form"),
                    Subject = emailSubject,
                    Body = emailBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                
                // Set reply-to as the person who contacted you
                mailMessage.ReplyToList.Add(new MailAddress(email, name));

                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation("Contact email sent successfully from {Name} <{Email}>", name, email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact email from {Name} <{Email}>: {Error}", name, email, ex.Message);
                return false;
            }
        }
    }
}
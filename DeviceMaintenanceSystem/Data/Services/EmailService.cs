using System.Net;
using System.Net.Mail;

namespace DeviceMaintenanceSystem.Data.Services
{
    public interface IEmailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string message
        );
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string message)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var enabled =
                _configuration.GetValue<bool>(
                    "EmailSettings:Enabled"
                );

            if (!enabled)
            {
                _logger.LogInformation(
                    "Email sending is disabled. Recipient: {Recipient}, Subject: {Subject}",
                    toEmail,
                    subject
                );

                return;
            }

            var host =
                _configuration["EmailSettings:SmtpHost"];

            var port =
                _configuration.GetValue<int>(
                    "EmailSettings:SmtpPort"
                );

            var username =
                _configuration["EmailSettings:Username"];

            var password =
                _configuration["EmailSettings:Password"];

            var fromEmail =
                _configuration["EmailSettings:FromEmail"];

            var fromName =
                _configuration["EmailSettings:FromName"]
                ?? "Taif University - Device Maintenance System";

            var enableSsl =
                _configuration.GetValue<bool>(
                    "EmailSettings:EnableSsl"
                );

            if (
                string.IsNullOrWhiteSpace(host) ||
                port <= 0 ||
                string.IsNullOrWhiteSpace(fromEmail)
            )
            {
                _logger.LogWarning(
                    "Email settings are incomplete. Email was not sent."
                );

                return;
            }

            try
            {
                using var mailMessage =
                    new MailMessage
                    {
                        From = new MailAddress(
                            fromEmail,
                            fromName
                        ),

                        Subject = subject,

                        Body = BuildHtml(message),

                        IsBodyHtml = true
                    };

                mailMessage.To.Add(toEmail);

                using var smtpClient =
                    new SmtpClient(host, port)
                    {
                        EnableSsl = enableSsl,
                        UseDefaultCredentials = false
                    };

                if (
                    !string.IsNullOrWhiteSpace(username) &&
                    !string.IsNullOrWhiteSpace(password)
                )
                {
                    smtpClient.Credentials =
                        new NetworkCredential(
                            username,
                            password
                        );
                }

                await smtpClient.SendMailAsync(
                    mailMessage
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email to {Recipient}.",
                    toEmail
                );
            }
        }

        private static string BuildHtml(
            string message)
        {
            return $"""
                <div style="font-family:Arial,sans-serif;background:#fbf8f2;padding:28px;color:#4e4031;">

                    <div style="max-width:620px;margin:auto;background:#ffffff;border:1px solid #e8e0d3;border-radius:16px;padding:28px;">

                        <h2 style="margin-top:0;color:#345b2f;">
                            Device Maintenance System
                        </h2>

                        <p style="line-height:1.8;font-size:15px;">
                            {WebUtility.HtmlEncode(message)}
                        </p>

                        <hr style="border:0;border-top:1px solid #e8e0d3;margin:24px 0;" />

                        <p style="margin:0;color:#756858;font-size:12px;">
                            Taif University - Technical Support Department
                        </p>

                    </div>

                </div>
                """;
        }
    }
}
using BasiQBLL.Services.Abstraction;
using BasiQBLL.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BasiQBLL.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IHostEnvironment hostEnvironment)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm Your Email Address";
            var body = await GetTemplateAsync("EmailConfirmation", new
            {
                ConfirmationLink = confirmationLink
            });

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation($"Email confirmation sent to {email}");
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = await GetTemplateAsync("PasswordReset", new
            {
                ResetLink = resetLink
            });

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation($"Password reset email sent to {email}");
        }

        public async Task SendNotificationAsync(string email, string subject, string message)
        {
            var body = await GetTemplateAsync("Notification", new
            {
                Message = message,
                Date = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt")
            });

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation($"Notification sent to {email}: {subject}");
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = $"Welcome to Our App, {userName}!";
            var body = await GetTemplateAsync("Welcome", new
            {
                UserName = userName,
                Year = DateTime.Now.Year
            });

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation($"Welcome email sent to {email}");
        }

        public async Task SendPasswordChangedNotificationAsync(string email)
        {
            var subject = "Your Password Has Been Changed";
            var body = await GetTemplateAsync("PasswordChanged", new
            {
                ChangeTime = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt")
            });

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation($"Password changed notification sent to {email}");
        }

        public async Task SendCustomEmailAsync(string email, string subject, string body, bool isHtml = true)
        {
            await SendEmailAsync(email, subject, body, isHtml);
            _logger.LogInformation($"Custom email sent to {email}: {subject}");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}. Subject: {subject}");
                throw new InvalidOperationException($"Failed to send email to {toEmail}", ex);
            }
        }

        private async Task<string> GetTemplateAsync(string templateName, object model)
        {
            // Try to load from file first
            var templatePath = Path.Combine(
                _hostEnvironment.ContentRootPath,
                _emailSettings.TemplatesPath,
                $"{templateName}.html");

            if (File.Exists(templatePath))
            {
                var template = await File.ReadAllTextAsync(templatePath);
                return ReplacePlaceholders(template, model);
            }

            // Fallback to embedded template if file doesn't exist
            return GetFallbackTemplate(templateName, model);
        }

        private string ReplacePlaceholders(string template, object model)
        {
            var result = template;
            var properties = model.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var placeholder = $"{{{{{prop.Name}}}}}";
                var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }

            return result;
        }

        private string GetFallbackTemplate(string templateName, object model)
        {
            return templateName switch
            {
                "EmailConfirmation" => GetEmailConfirmationFallback(model),
                "PasswordReset" => GetPasswordResetFallback(model),
                "Notification" => GetNotificationFallback(model),
                "Welcome" => GetWelcomeFallback(model),
                "PasswordChanged" => GetPasswordChangedFallback(model),
                _ => throw new ArgumentException($"Template {templateName} not found")
            };
        }

        private string GetEmailConfirmationFallback(object model)
        {
            var link = model.GetType().GetProperty("ConfirmationLink")?.GetValue(model)?.ToString() ?? "#";
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .button {{
                            display: inline-block; padding: 10px 20px; 
                            background-color: #007bff; color: white; 
                            text-decoration: none; border-radius: 5px; 
                            margin: 20px 0;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Confirm Your Email Address</h2>
                        <p>Thank you for registering! Please confirm your email address:</p>
                        <a href='{link}' class='button'>Confirm Email</a>
                        <p>If the button doesn't work, copy this link: {link}</p>
                        <p>This link will expire in 24 hours.</p>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordResetFallback(object model)
        {
            var link = model.GetType().GetProperty("ResetLink")?.GetValue(model)?.ToString() ?? "#";
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .button {{
                            display: inline-block; padding: 10px 20px; 
                            background-color: #28a745; color: white; 
                            text-decoration: none; border-radius: 5px; 
                            margin: 20px 0;
                        }}
                        .warning {{ color: #dc3545; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Reset Your Password</h2>
                        <p>Click below to reset your password:</p>
                        <a href='{link}' class='button'>Reset Password</a>
                        <p class='warning'>This link expires in 1 hour.</p>
                        <p>If you didn't request this, please ignore this email.</p>
                    </div>
                </body>
                </html>";
        }

        private string GetNotificationFallback(object model)
        {
            var message = model.GetType().GetProperty("Message")?.GetValue(model)?.ToString() ?? string.Empty;
            var date = model.GetType().GetProperty("Date")?.GetValue(model)?.ToString() ?? DateTime.Now.ToString();

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .message {{ 
                            background-color: #f8f9fa; 
                            padding: 15px; 
                            border-left: 4px solid #17a2b8;
                            margin: 20px 0;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Notification</h2>
                        <div class='message'>
                            {message}
                        </div>
                        <p><small>Sent: {date}</small></p>
                    </div>
                </body>
                </html>";
        }

        private string GetWelcomeFallback(object model)
        {
            var userName = model.GetType().GetProperty("UserName")?.GetValue(model)?.ToString() ?? "there";
            var year = model.GetType().GetProperty("Year")?.GetValue(model)?.ToString() ?? DateTime.Now.Year.ToString();

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            color: white; padding: 30px; text-align: center; 
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to Our App!</h1>
                        </div>
                        <div style='padding: 30px;'>
                            <h2>Hello {userName}!</h2>
                            <p>We're excited to have you on board.</p>
                            <p>Get started by exploring your dashboard.</p>
                        </div>
                        <div style='text-align: center; color: #666; font-size: 12px;'>
                            <p>© {year} Our App. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordChangedFallback(object model)
        {
            var changeTime = model.GetType().GetProperty("ChangeTime")?.GetValue(model)?.ToString() ?? DateTime.Now.ToString();

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .success {{ 
                            background-color: #d4edda; 
                            border: 1px solid #c3e6cb; 
                            color: #155724; 
                            padding: 15px; 
                            border-radius: 5px; 
                        }}
                        .warning {{ 
                            background-color: #fff3cd; 
                            border: 1px solid #ffeeba; 
                            color: #856404; 
                            padding: 10px; 
                            border-radius: 5px; 
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Password Changed Successfully</h2>
                        <div class='success'>
                            Your password was changed at {changeTime}
                        </div>
                        <div class='warning' style='margin-top: 20px;'>
                            <strong>Didn't make this change?</strong>
                            <p>Contact support immediately.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}

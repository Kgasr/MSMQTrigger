using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace MsmqTrigger
{
    public class TriggerEmail
    {
        // SMTP Configuration
        private readonly IConfiguration _configuration;

        private string smtpServer;
        private int smtpPort;
        private string smtpUser;
        private string smtpPass;
        private string fromAddress;
        private string toAddress;

        public TriggerEmail(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Assign default values in case configuration keys are null
            smtpServer = _configuration["SmtpServer"] ?? "smtp-mail.outlook.com"; // Default SMTP server
            smtpUser = _configuration["SmtpUser"] ?? "default_user@domain.com"; // Default user
            smtpPass = _configuration["SmtpPass"] ?? ""; // Default password
            fromAddress = _configuration["FromAddress"] ?? "no-reply@domain.com"; // Default from address
            toAddress = _configuration["ToAddress"] ?? "default_to@domain.com"; // Default to address

            // Use int.TryParse to avoid null reference and convert the port
            if (!int.TryParse(_configuration["SmtpPort"], out smtpPort))
            {
                smtpPort = 587; // Default port value if parsing fails
            }
        }


        // Method to send email
        public string SendEmail(List<string> emailAddresses,string messageLabel, string messageBody, List<string> attachmentPaths)
        {
            string subject = messageLabel; // Use the label as the subject
            string body = messageBody; // Use the body as the email body

            try
            {
                // Create a new instance of the SmtpClient class
                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    // Configure SMTP client
                    smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtpClient.EnableSsl = true; // Set to true if your SMTP server requires SSL

                    // Create a new MailMessage object
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromAddress),
                        Subject = subject,
                        Body = body
                    };
                    mailMessage.To.Add(toAddress);
                    
                    // Add all recipients to the email
                    foreach (var address in emailAddresses.Distinct())
                    {
                       if (!mailMessage.To.Contains(new MailAddress(address))) // Avoid duplicate To addresses
                        {
                            mailMessage.To.Add(address);
                        }

                    }

                    foreach (var attachmentPath in attachmentPaths)
                    {
                        if (File.Exists(attachmentPath))
                        {
                            Attachment attachment = new Attachment(attachmentPath);
                            mailMessage.Attachments.Add(attachment);
                        }
                    }

                    // Send the email
                    smtpClient.Send(mailMessage);
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Email trigger Failed..!!" + ex.Message);
            }
        }
    }
}

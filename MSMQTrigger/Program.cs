using System.Text;
using Microsoft.Extensions.Configuration;

using Logging;
using MsmqTrigger;

namespace MsmqTriggerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string messageLabel = args[0];                                  // MSMQ Trigger passes first argument as Label
            string encodedMessageBody = args[1];                            // MSMQ Trigger passes second argument as body

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();

            // Seperate log file for each message
            //string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, messageLabel + "-" + configuration["Logging:LogFile"]);

            // Single log file for all messages.
            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["Logging:LogFile"]);
            
            var logger = LoggerProvider.CreateLogger(fileName,Enum.Parse<LogLevel>(configuration["Logging:LogLevel"], true), configuration["Logging:LogFormat"]);
           
            try
            {            
                string messageBody = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessageBody));

                await logger.Log(LogLevel.Debug, "Message Label : " + messageLabel);
                await logger.Log(LogLevel.Debug, "Message Body : " + messageBody);

                await logger.Log(LogLevel.Information, $"{messageLabel} : Setting Up Email List & extracting attachments started");
                var (emailAddresses, attachmentPaths, cleanedMessageBody) = MsmqUtils.Utils.ExtractFieldsFromJson(messageBody);
                await logger.Log(LogLevel.Information, $"{messageLabel} : Setting Up Email List & extracting attachments Completed");

                await logger.Log(LogLevel.Information, $" {messageLabel} : Email Trigger Initiated");
                TriggerEmail triggerEmail = new(configuration.GetSection("SmtpSettings"));
                string result = triggerEmail.SendEmail(emailAddresses, messageLabel, cleanedMessageBody, attachmentPaths);
                await logger.Log(LogLevel.Information, $"{messageLabel} : Email Trigger Completed with status: {result}");
            }
            catch (Exception ex)
            {
                await logger.Log(LogLevel.Error, $"{messageLabel} : Process failed with error : ", ex);
            }

        }
    }
}

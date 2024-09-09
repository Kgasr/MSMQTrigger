using MsmqUtils;
using MsmqTrigger;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;


namespace MsmqTriggerApp
{
    class Program
    {
        static string source = "DefaultApp";        // Ensure source exists if app is unable to create due to permission issue
        static string logName = "System";
        private static void LogEventData(string msg, string eventLogEntryType)
        {
            EventLogEntryType type = EventLogEntryType.Information;         // by default log type as information

            // Check if the event source exists, if not, create it
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, logName);
            }
            if (eventLogEntryType == "error")
            {
                type = EventLogEntryType.Error;                             // change log type to error in case of any error
            }
            EventLog.WriteEntry(source, msg, type);
        }

        static void Main(string[] args)
        {
            try
            {
                string messageLabel = args[0];                                  // MSMQ Trigger passes first argument as Label
                string encodedMessageBody = args[1];                            // MSMQ Trigger passes second argument as body

                string encodedMessage = (string)encodedMessageBody;             // Decode the message body to get actual message
                string messageBody = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessage));

                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("settings.json")
                    .Build();

                source = configuration["EventLogging:EventSource"] ?? source; // Default SMTP server
                logName = configuration["EventLogging:LogName"] ?? logName; // Default user

                LogEventData(messageLabel, "info");
                LogEventData(messageBody, "info");


                LogEventData("Setting Up Email List & extracting attachments started", "info");
                var (emailAddresses, attachmentPaths, cleanedMessageBody) = Utils.ExtractFieldsFromJson(messageBody);
                LogEventData("Setting Up Email List & extracting attachments completed", "info");

                LogEventData("Email Trigger Initiated", "info");
                TriggerEmail triggerEmail = new(configuration);
                string result = triggerEmail.SendEmail(emailAddresses, messageLabel, cleanedMessageBody, attachmentPaths);
                LogEventData("Email Trigger Completed with status : " + result, "info");
            }
            catch (Exception ex)
            {
                LogEventData("Process failed with error : " + ex.Message, "error"); ;
            }

        }
    }
}

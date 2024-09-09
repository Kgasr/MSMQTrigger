using Newtonsoft.Json.Linq;

namespace MsmqUtils
{
    public class Utils
    {
        public static (List<string> emails, List<string> attachmentPaths, string cleanedJson) ExtractFieldsFromJson(string jsonString)
        {
            try
            {
                // Parse the JSON string
                JObject jsonObject = JObject.Parse(jsonString);

                // Extract both emails and attachment paths
                List<string> emails = ExtractEmailsFromJson(jsonObject);
                List<string> attachmentPaths = ExtractAttachmentPathsFromJson(jsonObject);

                // Remove any properties related to "Attachments Path"
                RemoveAttachmentPathsProperty(jsonObject);

                // Convert the cleaned JSON object to a string
                string cleanedJsonString = jsonObject.ToString();

                // Return the list of email addresses, attachment paths, and cleaned JSON as a string
                return (emails, attachmentPaths, cleanedJsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("Fields extractions Failed..!!" + ex.Message);
            }
        }

        private static List<string> ExtractEmailsFromJson(JObject jsonObject)
        {
            // List to hold the extracted email addresses
            List<string> emails = new();

            // Extract email values from keys containing "email"
            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Contains("email", StringComparison.OrdinalIgnoreCase))
                {
                    string email = property.Value.ToString();
                    emails.Add(email);
                }
            }

            // Return the list of email addresses as an array
            return emails;
        }

        private static List<string> ExtractAttachmentPathsFromJson(JObject jsonObject)
        {
            // List to hold the extracted attachment paths
            List<string> attachmentPaths = new();

            // Extract attachment paths from the specific property
            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Contains("Attachments Path", StringComparison.OrdinalIgnoreCase))
                {
                    var attachmentsToken = property.Value;
                    if (attachmentsToken != null && attachmentsToken.Type == JTokenType.Array)
                    {
                        foreach (var path in attachmentsToken)
                        {
                            attachmentPaths.Add(path.ToString());
                        }
                    }
                }
            }

            // Return the list of attachment paths
            return attachmentPaths;
        }

        private static void RemoveAttachmentPathsProperty(JObject jsonObject)
        {
            // Remove properties related to "Attachments Path"
            var propertiesToRemove = new List<JProperty>();

            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Contains("Attachments Path", StringComparison.OrdinalIgnoreCase))
                {
                    propertiesToRemove.Add(property);
                }
            }

            foreach (var property in propertiesToRemove)
            {
                property.Remove();
            }
        }
    }
}

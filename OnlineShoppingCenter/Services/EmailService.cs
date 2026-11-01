using Microsoft.Extensions.Options;
using OnlineShoppingCenter.Services;
using RestSharp;

namespace OnlineShoppingCenter.Services
{
    public class EmailRecipient
    {
        public string Email { get; set; } = string.Empty;

        public string? Name { get; set; }
    }

    public class EmailMessage
    {
        public string Subject { get; set; } = string.Empty;

        public string HtmlContent { get; set; } = string.Empty;

        public List<EmailRecipient> Recipients { get; set; } = new();
    }

    public class EmailService
    {
        private readonly RestClient _client;
        private readonly BrevoSettings _settings;

        public EmailService(IOptions<BrevoSettings> options)
        {
            _settings = options.Value;

            var clientOptions = new RestClientOptions(
                "https://api.brevo.com/v3/"
            );

            _client = new RestClient(clientOptions);
        }

        public async Task<bool> SendEmailAsync(EmailMessage email)
        {
            if (email.Recipients == null ||
                !email.Recipients.Any())
            {
                throw new ArgumentException(
                    "At least one recipient is required."
                );
            }

            var request = new RestRequest("smtp/email", Method.Post);

            // Authentication
            request.AddHeader("api-key", _settings.ApiKey);
            request.AddHeader("accept", "application/json");

            // Dynamic email data
            var emailData = new
            {
                sender = new
                {
                    name = _settings.SenderName,
                    email = _settings.SenderEmail
                },

                to = email.Recipients.Select(recipient => new
                {
                    email = recipient.Email,
                    name = recipient.Name
                }).ToList(),

                subject = email.Subject,

                htmlContent = email.HtmlContent
            };

            request.AddJsonBody(emailData);

            // Send request to Brevo
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception(
                    $"Email sending failed: {response.StatusCode} - {response.Content}"
                );
            }

            return true;
        }
    }
}

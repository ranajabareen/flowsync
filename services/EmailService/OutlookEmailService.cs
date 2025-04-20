
using Microsoft.Graph.Models;
using WebApplicationFlowSync.DTOs;

namespace WebApplicationFlowSync.services.EmailService
{
    public class OutlookEmailService : IEmailService
    {
        private readonly GraphAuthProvider _authProvider;
        public OutlookEmailService(GraphAuthProvider authProvider)
        {
            _authProvider = authProvider;
        }
        public async Task sendEmailAsync(EmailDto request)
        {
            try
            {
                var graphClient = _authProvider.GetPersonalAuthenticatedClient();

                var message = new Message
                {
                    Subject = request.Subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = request.Body
                    },
                    ToRecipients =
                    [
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = request.To
                            }
                        }
                    ]
                };

                await graphClient.Me.SendMail.PostAsync(new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}

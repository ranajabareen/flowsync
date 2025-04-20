
using Azure.Identity;
using Microsoft.Graph;

namespace WebApplicationFlowSync.services
{
    public class GraphAuthProvider
    {
        private readonly IConfiguration _config;

        public GraphAuthProvider(IConfiguration config)
        {
            _config = config;
        }

        public GraphServiceClient GetAuthenticatedClient()
        {
            var tenantId = _config["AzureAd:TenantId"];
            var clientId = _config["AzureAd:ClientId"];
            var clientSecret = _config["AzureAd:ClientSecret"];

            var scopes = new[] {
                "https://graph.microsoft.com/.default",
                "https://graph.microsoft.com/Mail.Send",
                "https://graph.microsoft.com/Mail.ReadWrite"
            };
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };



            // Define authentication provider
            var clientSecretCredential = new ClientSecretCredential(
                tenantId,
                clientId,
                clientSecret, options);

            var accessToken = clientSecretCredential.GetToken(new Azure.Core.TokenRequestContext(scopes) { });
            Console.WriteLine(accessToken.Token);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }

        public GraphServiceClient GetPersonalAuthenticatedClient()
        {
            var clientId = _config["AzureAd:ClientId"];

            var tenantId = "common";

            var options = new InteractiveBrowserCredentialOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                RedirectUri = new Uri("http://localhost"),
            };

            var credential = new InteractiveBrowserCredential(options);
            var graphClient = new GraphServiceClient(credential, new[] { "User.Read", "Mail.Send" });
            return graphClient;
        }
        
    }
}



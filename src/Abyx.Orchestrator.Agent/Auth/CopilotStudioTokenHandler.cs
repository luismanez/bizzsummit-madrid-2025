using System.Net.Http.Headers;
using Azure.Identity;

namespace Abyx.Orchestrator.Agent.Auth;

public class CopilotStudioTokenHandler : HttpClientHandler
{
    private readonly string _tenantId;
    private readonly string _clientId;

    public CopilotStudioTokenHandler(string tenantId, string clientId)
    {
        _tenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var scopes = new[] { "https://api.gov.powerplatform.microsoft.us/CopilotStudio.Copilots.Invoke" };

            // using Azure.Identity;
            var options = new InteractiveBrowserCredentialOptions
            {
                TenantId = _tenantId,
                ClientId = _clientId,
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                RedirectUri = new Uri("http://localhost"),
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.interactivebrowsercredential
            var interactiveCredential = new InteractiveBrowserCredential(options);
            var accessToken = await interactiveCredential.GetTokenAsync(
                new Azure.Core.TokenRequestContext(scopes),
                cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

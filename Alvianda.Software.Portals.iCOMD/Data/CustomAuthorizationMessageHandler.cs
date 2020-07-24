using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace Alvianda.Software.Portals.iCOMD.Data
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(
            IConfiguration config,
            IAccessTokenProvider provider,
            NavigationManager navigation) : base(provider, navigation)
        {
            var section = config.GetSection(nameof(TokenClient));
            var endpoint = section.GetValue<string>(nameof(TokenClient.Endpoint));
            ConfigureHandler(new[] { endpoint });
        }
    }
}

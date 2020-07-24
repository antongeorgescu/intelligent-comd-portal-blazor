using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Alvianda.Software.Portals.COMD.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Alvianda.Software.Portals.COMD
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
                        
            // set up a delegate to get the endpoint of EventViewerLogger API 
            // to fetch the token
            static string evLoggerApiId(WebAssemblyHostBuilder builder) =>
                builder.Configuration
                    .GetSection("AzureAD")
                    .GetValue<string>("EventViewLoggerApi_AppId");

            // set up a delegate to get the authenticate authority 
            // to fetch the token
            static string authenticateAuthority(WebAssemblyHostBuilder builder) =>
                builder.Configuration
                    .GetSection("AzureAD")
                    .GetValue<string>("Authority");

            // set up a delegate to get the COMP Client ID
            // to fetch the token
            static string compClientId(WebAssemblyHostBuilder builder) =>
                builder.Configuration
                    .GetSection("AzureAD")
                    .GetValue<string>("COMP_UI_ClientId");

            //builder.Services.AddBlazoredLocalStorage();
            
            // Configure the default client(talks to own domain)
            var client = new HttpClient()
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };
            builder.Services.AddTransient(sp => client);

            builder.Services.AddMsalAuthentication(options =>
            {
                var authentication = options.ProviderOptions.Authentication;
                //authentication.Authority = "https://login.microsoftonline.com/e8422127-880e-4288-928e-4ced14423628";
                //authentication.ClientId = "dfd5e247-c98d-4c1d-bd23-e0a75f9b894a";
                //options.ProviderOptions.DefaultAccessTokenScopes.Add("api://45e30ce1-edb0-45fb-a6e2-39412f86c348/API.LogReader");

                authentication.Authority = $"{authenticateAuthority(builder)}";
                authentication.ClientId = $"{compClientId(builder)}";
                options.ProviderOptions.DefaultAccessTokenScopes.Add($"{evLoggerApiId(builder)}");

            });

            //Microsoft.AspNetCore.Hosting.IHostingEnvironment env = null; // Microsoft.AspNetCore.Hosting.WebHostDefaults;
            var eventViewerService = new EventViewerService(client, builder.Configuration);
            builder.Services.AddTransient<EventViewerService>(sp => eventViewerService);

            // set up the authorization handler to inject tokens
            //builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

            // configure the client to talk to the Azure Functions endpoint.
            //builder.Services.AddHttpClient(nameof(TokenClient),
            //    client =>
            //    {
            //        client.BaseAddress = new Uri("http://localhost:53534/");
            //    }).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

            // Use MyAuthenticationStateProvider as the AuthenticationStateProvider
            //builder.Services.AddScoped<AuthenticationStateProvider, MyAuthenticationStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}

using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace opisense_sample_dotnet_console
{
    public class Authenticator
    {
        private static readonly string IdentityServer = ConfigurationManager.AppSettings["OpisenseIdentity"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["OpisenseClientId"];
        private static readonly string ClientSecret = ConfigurationManager.AppSettings["OpisenseClientSecret"];

        private static readonly string Username = ConfigurationManager.AppSettings["OpisenseUsername"];
        private static readonly string Password = ConfigurationManager.AppSettings["OpisensePassword"];

        /// <summary>
        /// Access token should be stored and use accross calls, and the refresh token should be used to review the Access token
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetBearerToken()
        {
            var scopes = new[] { "openid", "opisense-api", "push-data" };
            var response = await new TokenClient($"{IdentityServer}connect/token", ClientId, ClientSecret)
                .RequestResourceOwnerPasswordAsync(Username, Password, string.Join(" ", scopes));
            if (response.IsError)
            {
                throw new Exception(response.Error);
            }
            return response.AccessToken;
        }

        public async Task<HttpClient> GetAuthenticatedClient()
        {
            var bearerToken = await GetBearerToken();
            var client = new HttpClient();
            client.SetBearerToken(bearerToken);
            return client;
        }
    }
}
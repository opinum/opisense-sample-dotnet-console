using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
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
        private static readonly string AskForCredentials = ConfigurationManager.AppSettings["AskForCredentials"];

        /// <summary>
        /// Access token should be stored and use accross calls, and the refresh token should be used to review the Access token
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetBearerToken()
        {
            var username = Username;
            var password = Password;
            if (!string.IsNullOrWhiteSpace(AskForCredentials) && bool.Parse(AskForCredentials))
            {
                Console.WriteLine("Please enter your Username:");
                username = Console.ReadLine();
                Console.WriteLine("Please enter your Password:");
                password = GetConsolePassword();
            }

            var scopes = new[] { "openid", "opisense-api", "push-data" };
            var response = await new TokenClient($"{IdentityServer}connect/token", ClientId, ClientSecret)
                .RequestResourceOwnerPasswordAsync(username, password, string.Join(" ", scopes));
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

        private static string GetConsolePassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }

                    continue;
                }

                Console.Write('*');
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }
    }
}
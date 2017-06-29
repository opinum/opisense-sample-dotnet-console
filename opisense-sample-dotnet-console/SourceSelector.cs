using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleTables;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    public class SourceSelector
    {
        private readonly SiteSelector siteSelector;
        private readonly Authenticator authenticator;
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];

        public SourceSelector(SiteSelector siteSelector, Authenticator authenticator)
        {
            this.siteSelector = siteSelector;
            this.authenticator = authenticator;
        }

        public async Task<Source[]> DisplaySources(bool fromSite = false, HttpClient client = null)
        {
            if (client == null)
            {
                using (client = await authenticator.GetAuthenticatedClient())
                {
                    return await InternalDisplaySource(client, fromSite);
                }
            }
            return await InternalDisplaySource(client, fromSite);
        }

        private static async Task<Source[]> GetSources(HttpClient client, int? siteId)
        {
            var siteFilter = siteId == null ? "" : $"&siteId={siteId}";
            var response = await client.GetAsync($"{OpisenseApi}sources?displayLevel=site{siteFilter}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Source[]>();
        }

        private async Task<Source[]> InternalDisplaySource(HttpClient client, bool fromSite = false)
        {
            int? siteId = null;
            if (fromSite)
            {
                await siteSelector.DisplaySite(client);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Enter the Id of the site you want to select: ");

                siteId = Convert.ToInt32(Console.ReadLine());
            }

            var sources = await GetSources(client, siteId);

            ConsoleTable
                .From(sources)
                .Write();

            return sources;
        }
    }
}
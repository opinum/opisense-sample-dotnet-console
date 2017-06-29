using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleTables;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    public class SiteSelector
    {
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];
        
        private static async Task<Site[]> GetSites(HttpClient client)
        {
            var response = await client.GetAsync($"{OpisenseApi}sites?displayLevel=site");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Site[]>();
        }

        public async Task<Site[]> DisplaySite(HttpClient client)
        {
            var sites = await GetSites(client);

            ConsoleTable
                .From(sites)
                .Write();

            return sites;
        }
    }
}
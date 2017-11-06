using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleTables;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    public class SiteDeletor
    {
        private readonly Authenticator authenticator;
        private readonly SiteSelector siteSelector;
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];

        public SiteDeletor(SiteSelector siteSelector, Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.siteSelector = siteSelector;
        }

        public async Task DeleteSite()
        {
            var client = await authenticator.GetAuthenticatedClient();
            var sites = await siteSelector.DisplaySite(client);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Enter the Id of the site you want to delete: ");

            var siteId = Convert.ToInt32(Console.ReadLine());
            var site = sites.Single(x => x.Id == siteId);

            Console.WriteLine("Chosen Site: ");
            ConsoleTable
                .From(new List<Site> { site })
                .Write();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Are you sure you want to delete this site?");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Type YES if you confirm:");
            var response = Console.ReadLine();
            if (response != null && response.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: This action cannot be undone, please enter the Site Id to validate:");
                var confirmId = Convert.ToInt32(Console.ReadLine());
                if (confirmId == siteId)
                {
                    await InternalDeleteSite(client, siteId);

                }
                else
                {
                    Console.WriteLine("The site id you typed is different. Abording the request.");
                }
            }
        }

        private async Task InternalDeleteSite(HttpClient client, int siteId)
        {
            var response = await client.DeleteAsync($"{OpisenseApi}sites/{siteId}");
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(response.ReasonPhrase);
                throw;
            }
        }
    }
}
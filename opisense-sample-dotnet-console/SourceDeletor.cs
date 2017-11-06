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
    public class SourceDeletor
    {
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];
        private readonly SourceSelector sourceSelector;
        private readonly Authenticator authenticator;

        public SourceDeletor(SourceSelector sourceSelector, Authenticator authenticator)
        {
            this.sourceSelector = sourceSelector;
            this.authenticator = authenticator;
        }

        public async Task DeleteSource()
        {
            var client = await authenticator.GetAuthenticatedClient();
            var sources = await sourceSelector.DisplaySources(true);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Enter the Id of the source you want to delete: ");

            var sourceId = Convert.ToInt32(Console.ReadLine());
            var source = sources.Single(x => x.Id == sourceId);

            Console.WriteLine("Chosen source: ");
            ConsoleTable
                .From(new List<Source> { source })
                .Write();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Are you sure you want to delete this source?");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Type YES if you confirm:");
            var response = Console.ReadLine();

            if (response != null && response.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: This action cannot be undone, please enter the Source Id to validate:");
                var confirmId = Convert.ToInt32(Console.ReadLine());
                if (confirmId == sourceId)
                {
                    await InternalDeleteSource(client, sourceId);

                }
                else
                {
                    Console.WriteLine("The source id you typed is different. Abording the request.");
                }
            }
        }

        private static async Task InternalDeleteSource(HttpClient client, int sourceId)
        {
            var response = await client.DeleteAsync($"{OpisenseApi}sources/{sourceId}");
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
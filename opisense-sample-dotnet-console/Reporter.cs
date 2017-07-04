using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleTables;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    public class Reporter
    {
        private readonly VariableSelector variableSelector;
        private readonly Authenticator authenticator;
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];

        public Reporter(VariableSelector variableSelector,Authenticator authenticator)
        {
            this.variableSelector = variableSelector;
            this.authenticator = authenticator;
        }

        public async Task DisplayData()
        {
            using (var client = await authenticator.GetAuthenticatedClient())
            {
                var variable = await variableSelector.SelectVariable(client);
                if (variable == null) return;

                var variableTypes = await GetVariableTypes(client);
                var variableType = variableTypes.Single(x => x.Id == variable.VariableTypeId);

                Console.WriteLine();
                Console.WriteLine("1. Get RAW Data");
                Console.WriteLine("2. Get Data grouped by hour");

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Select how to get the data: ");
                var display = Convert.ToInt32(Console.ReadLine());

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var data = await GetData(client, variable, display == 1 ? 0 : 2, variableType.Aggregate);
                var getDataElapsedTime = sw.Elapsed;
                sw.Restart();
                ConsoleTable
                    .From(data.Select(x => new { x.VariableId, Value = x.GetValue(), x.Date }))
                    .Write();
                var displayDataElapsedTime = sw.Elapsed;
                sw.Stop();
                Console.WriteLine($"Data retrieval took {getDataElapsedTime:g}");
                Console.WriteLine($"Data display took {displayDataElapsedTime:g}");
            }
        }

        private async Task<VariableType[]> GetVariableTypes(HttpClient client)
        {
            var response = await client.GetAsync($"{OpisenseApi}variableTypes");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<VariableType[]>();
        }

        private static async Task<ValueData[]> GetData(HttpClient client, Variable variable, int granularity, int aggregate)
        {
            var aggregation = granularity == 0 ? "" : $"&aggregation={aggregate}";
            var response = await client.GetAsync($"{OpisenseApi}data?displayLevel=ValueVariableDate&granularity={granularity}&variableId={variable.Id}{aggregation}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<ValueData[]>();
        }

    }
}
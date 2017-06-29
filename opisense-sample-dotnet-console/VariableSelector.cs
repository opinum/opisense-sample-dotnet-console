using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleTables;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    public class VariableSelector
    {
        private readonly SourceSelector sourceSelector;
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];

        public VariableSelector(SourceSelector sourceSelector)
        {
            this.sourceSelector = sourceSelector;
        }

        public async Task<Variable> SelectVariable(HttpClient client)
        {
            await sourceSelector.DisplaySources();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Enter the Id of the source for which you want see the data: ");

            var sourceId = Convert.ToInt32(Console.ReadLine());

            var variables = await DisplayVariables(client, sourceId);
            Console.WriteLine();
            Console.WriteLine();
            if (variables.Length == 0)
            {
                Console.WriteLine("No variable on the selected source...");
                return null;
            }

            Console.WriteLine("Enter the Id of the variable for which you want see the data: ");

            var variableId = Convert.ToInt32(Console.ReadLine());
            var variable = variables.Single(x => x.Id == variableId);
            return variable;
        }

        private static async Task<Variable[]> GetVariables(HttpClient client, int sourceId)
        {
            var response = await client.GetAsync($"{OpisenseApi}variables/source/{sourceId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Variable[]>();
        }

        private static async Task<Variable[]> DisplayVariables(HttpClient client, int sourceId)
        {
            var variables = await GetVariables(client, sourceId);

            ConsoleTable
               .From(variables)
               .Write();

            return variables;
        }
    }
}
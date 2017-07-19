using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleTables;
using opisense_sample_dotnet_console.Model;
using System.Text.RegularExpressions;

namespace opisense_sample_dotnet_console
{
	public class CleanUpper
	{
		private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];
		private readonly Authenticator authenticator;

		public CleanUpper(Authenticator authenticator)
		{
			this.authenticator = authenticator;
		}

		public async Task CleanupTestSitesAndData()
		{
			using (var client = await authenticator.GetAuthenticatedClient())
			{
				var sites = await GetSites(client, @"(^[S|s]ite[1-3]$)|^[T|t]est*");
				ConsoleTable
					.From(sites)
					.Write();

				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("Are you sure you want to delete completely all sites with sources and data?");
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("Type YES if you confirm:");
				var response = Console.ReadLine();

				if (response != null && response.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
				{
					Console.WriteLine();
					Console.WriteLine("WARNING: This action cannot be undone, please enter SURE to validate:");
					response = Console.ReadLine();
					if (response != null && response.Equals("SURE", StringComparison.InvariantCultureIgnoreCase))
					{
						await InternalCleanUp(client, sites);

					}
					else
					{
						Console.WriteLine("Abording the request.");
					}
				}
			}
		}

		private static async Task<Site[]> GetSites(HttpClient client, string siteNameFilterRegEx)
		{
			var response = await client.GetAsync($"{OpisenseApi}sites?displayLevel=site");
			response.EnsureSuccessStatusCode();

			var myRegex = new Regex(siteNameFilterRegEx);
			return (await response.Content.ReadAsAsync<Site[]>()).Where(x => myRegex.IsMatch(x.Name)).ToArray();
		}

		private static async Task<Source[]> GetSources(HttpClient client, int? siteId)
		{
			var siteFilter = siteId == null ? "" : $"&siteId={siteId}";
			var response = await client.GetAsync($"{OpisenseApi}sources?displayLevel=site{siteFilter}");
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsAsync<Source[]>();
		}

		private static async Task InternalCleanUp(HttpClient client, Site[] sites)
		{
			foreach (var i in sites)
			{
				Console.WriteLine();
				var sources = await GetSources(client, i.Id);

				foreach (var j in sources)
				{
					Console.WriteLine($"Deleting source {j.Id}: {j.Name}");
					await InternalDeleteSource(client, j.Id);
				}

				Console.WriteLine($"Deleting site {i.Id}: {i.Name}");
				await InternalDeleteSite(client, i.Id);
			}
		}

		private static async Task InternalDeleteSource(HttpClient client, int sourceId)
		{
			//return;
			//var response = await client.DeleteAsync($"{OpisenseApi}sources/{sourceId}?options.hardDelete=true");
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

		private static async Task InternalDeleteSite(HttpClient client, int siteId)
		{
			//return;
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    public class DataCreator
    {
        private static readonly string OpisensePush = ConfigurationManager.AppSettings["OpisensePush"];
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];
        private static readonly string DefaultTimezone = ConfigurationManager.AppSettings["DefaultTimezone"];
        private readonly VariableSelector variableSelector;
        private readonly Authenticator authenticator;

        public DataCreator(VariableSelector variableSelector, Authenticator authenticator)
        {
            this.variableSelector = variableSelector;
            this.authenticator = authenticator;
        }

        public async Task DemoSetup()
        {
            using (var client = await authenticator.GetAuthenticatedClient())
            {
                var form = new Form
                {
                    Name = "SOURCE_FORM",
                    EntityType = "source",
                    Groups = new List<Group>
                    {
                        new Group
                        {
                            Name = "Group1",
                            Fields = new List<Field>
                            {
                                new Field{Name = "STRING_FIELD", Type = FieldType.String},
                                new Field{Name = "DOUBLE_FIELD", Type = FieldType.Double},
                            }
                        },
                        new Group
                        {
                            Name = "Group2",
                            Fields = new List<Field>
                            {
                                new Field{Name = "INT_LIST_FIELD", Type = FieldType.Int, IsList = true, Items = new List<ListItem>
                                {
                                    new ListItem{Name = "One", Value = 1},
                                    new ListItem{Name = "Two", Value = 2}

                                }},
                                new Field{Name = "DATE_FIELD", Type = FieldType.Date},
                            }
                        }

                    }
                };
                Console.WriteLine("Checking if source form exists");
                if (await GetSourceForm(client, form.Name) == null)
                {
                    Console.WriteLine("Creating source form");
                    await CreateSourceForm(client, form);
                }



                Console.WriteLine("Creating 3 sites in Opisense");
                Console.WriteLine("Creating site1");
                var siteId1 = await CreateSite(client, "site1");
                Console.WriteLine("Creating site2");
                var siteId2 = await CreateSite(client, "site2");
                Console.WriteLine("Creating site3");
                var siteId3 = await CreateSite(client, "site3");

                Console.WriteLine("Creating source1 and variables");
                var source1 = await CreateSource(client, siteId1, "source1");
                // Add 
                var source1Variable1 = await CreateVariable(client, source1.Id, 0);
                // Ajout de la variable d'index
                var source1Variable2 = await CreateVariable(client, source1.Id, 25);

                Console.WriteLine("Creating source2 and variables");
                var source2 = await CreateSource(client, siteId1, "source2");
                // Ajout de la variable de consommation
                var source2Variable1 = await CreateVariable(client, source2.Id, 0);
                // Ajout de la variable d'index
                var source2Variable2 = await CreateVariable(client, source2.Id, 25);

                Console.WriteLine("Creating source3 and variables");
                var source3 = await CreateSource(client, siteId2, "source3");
                // Ajout de la variable de consommation
                var source3Variable1 = await CreateVariable(client, source3.Id, 0);
                // Ajout de la variable d'index
                var source3Variable2 = await CreateVariable(client, source3.Id, 25);

                Console.WriteLine("Creating source4 and variables");
                var source4 = await CreateSource(client, siteId2, "source4");
                // Ajout de la variable de consommation
                var source4Variable1 = await CreateVariable(client, source4.Id, 0);
                // Ajout de la variable d'index
                var source4Variable2 = await CreateVariable(client, source4.Id, 25);

                Console.WriteLine("Creating source5 and variables");
                var source5 = await CreateSource(client, siteId3, "source5");
                // Ajout de la variable de consommation
                var source5Variable1 = await CreateVariable(client, source5.Id, 0);
                // Ajout de la variable d'index
                var source5Variable2 = await CreateVariable(client, source5.Id, 25);


                //    Ajouter dans chaque source précédemment créé 2 variables(type double et type int) avec 1 ans d'historique des valeurs au points de 10mins (=6*24*365*2 points), sauf pour le site3 ou il faut avoir un "trou" pour tous les 10 valeurs

                Console.WriteLine("Adding data in source 1");
                await CreateData(client, source1Variable1, source1Variable2);
                Console.WriteLine("Adding data in source 2");
                await CreateData(client, source2Variable1, source2Variable2);
                Console.WriteLine("Adding data in source 3");
                await CreateData(client, source3Variable1, source3Variable2);
                Console.WriteLine("Adding data in source 4");
                await CreateData(client, source4Variable1, source4Variable2);
                Console.WriteLine("Adding data in source 5");
                await CreateData(client, source5Variable1, source5Variable2, holesEveryXDatapoint: 10);


                // ATTENTION: LORS DE L'INGESTION, LES DONNEES PASSENT PAS PLUSIEURS ETAPES (ENRICHISSEMENT, VARIABLE CALCULEES, ALERTES)
                // IL Y A DONC UN DELAIS POUVANT ALLER JUSQU'A 10 MINUTES AVANT QU'ELLES SOIENT ACCESSIBLE VIA L'API
            }
        }

        private async Task CreateSourceForm(HttpClient client, Form form)
        {
            var response = await client.PostAsJsonAsync($"{OpisenseApi}form", form);
            response.EnsureSuccessStatusCode();
        }

        private async Task<object> GetSourceForm(HttpClient client, string name)
        {
            var response = await client.GetAsync($"{OpisenseApi}form?name={HttpUtility.UrlEncode(name)}");
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadAsAsync<List<Form>>()).FirstOrDefault();
        }

        public async Task UpdateData()
        {
            //Pour le site1 source1 variable1 mettre à jour un an des valeurs
            //    Pour le site1 source1 dupliquer tous les valeurs de variable1 en variable3
            using (var client = await authenticator.GetAuthenticatedClient())
            {
                var variable = await variableSelector.SelectVariable(client);
                Console.WriteLine($"Generating data for variable id {variable.Id}");
                var data = GenerateData(DateTime.Today.AddYears(-1), DateTime.Today, TimeSpan.FromMinutes(10), 100);
                await AddOrUpdateData(client, variable.Id, data);
            }
        }

        private async Task CreateData(HttpClient client, Variable variable1, Variable variable2, int? holesEveryXDatapoint = null)
        {
            Console.WriteLine($"Generating data for variable id {variable1.Id} and {variable2.Id}");
            var data = GenerateData(DateTime.Today.AddYears(-1), DateTime.Today, TimeSpan.FromMinutes(10), 100, holesEveryXDatapoint);
            await AddOrUpdateData(client, variable1.Id, data);
            var incrementalData = FakeIndexes(data);
            await AddOrUpdateData(client, variable2.Id, incrementalData);
        }

        private List<Data> FakeIndexes(List<Data> data)
        {
            var result = new List<Data>();
            Data previousValue = null;
            foreach (var value in data)
            {
                if (previousValue == null)
                    previousValue = new Data { Date = value.Date, Value = 0 };
                else
                    previousValue = new Data { Date = value.Date, Value = previousValue.Value + value.Value };
                result.Add(previousValue);
            }
            return result;
        }

        private List<Data> GenerateData(DateTime from, DateTime today, TimeSpan period, int maxValue, int? holesEveryXDatapoint = null)
        {
            var result = new List<Data>();
            var rand = new Random();

            for (var date = from; date < today; date = date + period)
            {
                result.Add(new Data { Date = DateTime.SpecifyKind(date, DateTimeKind.Utc), Value = rand.NextDouble() * maxValue });
            }

            return holesEveryXDatapoint == null
                ? result
                : result.Where((r, i) => i % holesEveryXDatapoint != 0).ToList();
        }

        private async Task AddOrUpdateData(HttpClient client, int variableId, List<Data> data)
        {
            Console.WriteLine($"Importing {data.Count} datapoints for variable id {variableId}");
            var response = await client.PostAsJsonAsync($"{OpisensePush}standard", new
            {
                Data = data.Select(d => new { VariableId = variableId, Date = d.Date, Value = d.Value }).ToList()
            });
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Finished importing data for variable id {variableId}");
        }

        private static async Task<Variable> CreateVariable(HttpClient client, int sourceId, int variableTypeId)
        {
            return await CreateVariable(client, sourceId, new Variable
            {
                VariableTypeId = variableTypeId,
                UnitId = 8,
                Divider = 1,
                Granularity = 10,
                GranularityTimeBase = TimePeriod.Minute
            });
        }

        internal static async Task<Variable> CreateVariable(HttpClient client, int sourceId, object variable)
        {
            var response = await client.PostAsJsonAsync($"{OpisenseApi}variables/source/{sourceId}", variable);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Variable>();
        }

        private static async Task<Source> CreateSource(HttpClient client, int siteId, string sourceName)
        {
            return await CreateSource(client, new
            {
                Name = sourceName,
                SiteId = siteId,
                TimeZoneId = DefaultTimezone,
                SourceTypeId = 72,
                EnergyTypeId = 1,
                ClientData = new
                {
                    SOURCE_FORM = new
                    {
                        Group1 = new { STRING_FIELD = sourceName },
                        Group2 = new { INT_LIST_FIELD = 2, DATE_FIELD = DateTime.UtcNow }
                    }

                }
            });
        }

        internal static async Task<Source> CreateSource(HttpClient client, object source)
        {
            var response = await client.PostAsJsonAsync($"{OpisenseApi}sources", source);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Source>();
        }

        private static async Task<int> CreateSite(HttpClient client, string siteName)
        {
            //TypeID can be retrieved from GET /sitetypes
            //TimeZoneId can be retrieved from GET /timezones
            return await CreateSite(client, new { Name = siteName, TypeId = 1, TimeZoneId = DefaultTimezone, City = "Mont-Saint-Guibert", Country = "Belgium", Street = "Rue Emile Francqui, 6", PostalCode = "1435" });
        }

        internal static async Task<int> CreateSite(HttpClient client, object site)
        {
            var response = await client.PostAsJsonAsync($"{OpisenseApi}sites", site);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<int>();
        }
    }

    internal class Form
    {
        public string Name { get; set; }
        public string EntityType { get; set; } = "site";
        public List<Group> Groups { get; set; }
    }

    internal class Group
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
    }

    internal class Field
    {
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public bool IsList { get; set; }
        public List<ListItem> Items { get; set; }
    }
    public enum FieldType
    {
        String = 0,
        Int = 1,
        Double = 2,
        Date = 5,
    }

    internal class ListItem
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
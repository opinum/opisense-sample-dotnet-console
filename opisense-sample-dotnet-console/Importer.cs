using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    internal class Importer
    {
        private readonly Authenticator authenticator;

        public Importer(Authenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public async Task ImportDefinitions()
        {
            Console.WriteLine("Please enter the .json file containing the sites and sources");
            var fileName = Console.ReadLine();

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Could not open the file <{fileName}>");
                return;
            }
            Console.WriteLine($"Opening file {fileName}");
            var json = File.ReadAllText(fileName);
            try
            {
                var sites = JsonConvert.DeserializeObject<List<ImportSite>>(json);
                using (var client = await authenticator.GetAuthenticatedClient())
                {
                    foreach (var importSite in sites)
                    {
                        var siteId = await DataCreator.CreateSite(client, importSite);
                        Console.WriteLine($"Created site - id <{siteId}> named <{importSite.Name}>");
                        foreach (var importSource in importSite.Sources)
                        {
                            var source = JsonConvert.DeserializeObject<ImportSourceWithSiteId>(JsonConvert.SerializeObject(importSource));
                            source.SiteId = siteId;
                            var sourceId = (await DataCreator.CreateSource(client, source)).Id;
                            Console.WriteLine($"Created source - id <{sourceId}> named <{importSource.Name}> in site <{importSite.Name}>");

                            foreach (var importVariable in importSource.Variables)
                            {
                                var variable = await DataCreator.CreateVariable(client, sourceId, importVariable);
                                Console.WriteLine($"Created variable - id <{variable.Id}> of type <{importVariable.VariableTypeId}> source <{importSource.Name}> in site <{importSite.Name}>");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to deserialize the file <{fileName}>");
                Console.WriteLine($"Please addapt your file based on the following example");
                var clientData = JsonConvert.DeserializeObject<dynamic>(@"
                        {
                           ""MyForm1"":
                                {
                                ""MyGroup1"":
                                    {
                                    ""MyField1"":10,
                                    ""MyField2"": ""something""
                                    }
                                },
                            ""MyForm2"": { }
                        }");
                ImportSite sampleSite = new ImportSite
                {
                    Name = "My site",
                    Street = "Albert 1er",
                    City = "Bruxelles",
                    PostalCode = "1000",
                    Country = "Belgium",
                    TimeZoneId = "Romance Standard Time",
                    TypeId = 1,
                    ClientData = clientData,
                    Sources = new List<ImportSource>
                    {
                        new ImportSource
                        {
                            Name = "My Source",
                            Description = "Source description",
                            SourceTypeId = 72,
                            TimeZoneId = "Romance Standard Time",
                            EnergyTypeId = 1,
                            Variables = new List<VariableImport>
                            {
                                new VariableImport
                                {
                                    VariableTypeId = 0,
                                    UnitId = 8,
                                    Divider = 1,
                                    Granularity = 10,
                                    GranularityTimeBase = TimePeriod.Minute
                                },
                                new VariableImport
                                {
                                    VariableTypeId = 25,
                                    UnitId = 8,
                                    Divider = 1,
                                    Granularity = 10,
                                    GranularityTimeBase = TimePeriod.Minute
                                }

                            }
                        }
                    }
                };
                Console.WriteLine(JsonConvert.SerializeObject(new[] { sampleSite }, Formatting.Indented));
            }

        }
    }

    public class ImportSite
    {
        public string Name { get; set; }
        public int TypeId { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string TimeZoneId { get; set; }
        public dynamic ClientData { get; set; }

        public List<ImportSource> Sources { get; set; } = new List<ImportSource>();
    }

    public class ImportSource
    {
        public int EnergyTypeId { get; set; }
        public int? EnergyUsageId { get; set; }
        public string Name { get; set; }
        public int SourceTypeId { get; set; }
        public string TimeZoneId { get; set; }
        public string EanNumber { get; set; }
        public string MeterNumber { get; set; }
        public string Localisation { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string SerialNumber { get; set; }
        public int? DisplayVariableTypeId { get; set; }
        public string MeterAddress { get; set; }
        public List<VariableImport> Variables { get; set; } = new List<VariableImport>();
    }

    public class VariableImport
    {
        public int VariableTypeId { get; set; }
        public int UnitId { get; set; }
        public int Divider { get; set; }
        public double Granularity { get; set; }
        public TimePeriod GranularityTimeBase { get; set; }
    }

    public class ImportSourceWithSiteId : ImportSource
    {
        public int SiteId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Marvin.JsonPatch.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task ImportSites()
        {
            var json = LoadFile();
            if (json == null) return;
            try
            {
                using (var client = await authenticator.GetAuthenticatedClient())
                {
                    JArray array = JArray.Parse(json);
                    foreach (var jtoken in array)
                    {
                        var importSite = jtoken.ToObject<ImportSite>();

                        if (importSite.Id.HasValue)
                        {
                            var sitePatch = new JsonPatchDocument();
                            WalkNode(jtoken, "", (n, path) =>
                            {
                                foreach (var property in n.Properties())
                                {
                                    if (property.Value.Type != JTokenType.Array && property.Value.Type != JTokenType.Object && property.Value.Type != JTokenType.Property
                                        && !string.Equals(property.Name, nameof(ImportSite.Id), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        sitePatch.Replace($"{path}{property.Name}", property.ToObject(GetPropertyType(property.Value.Type)));
                                    }
                                }

                            });
                            await DataCreator.PatchSite(client, importSite.Id.Value, sitePatch);
                            Console.WriteLine($"Updated site - id <{importSite.Id}> named <{importSite.Name}>");
                        }
                        else
                        {
                            importSite.Id = await DataCreator.CreateSite(client, importSite);
                            Console.WriteLine($"Created site - id <{importSite.Id}> named <{importSite.Name}>");
                        }
                        var sources = jtoken[nameof(ImportSite.Sources)];
                        if (sources != null)
                        {
                            await ImportSources(client, sources, importSite.Id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to deserialize the file");
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

        public async Task ImportSources()
        {
            var json = LoadFile();
            if (json == null) return;

            try
            {
                using (var client = await authenticator.GetAuthenticatedClient())
                {
                    JArray array = JArray.Parse(json);
                    await ImportSources(client, array);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to deserialize the file");
                Console.WriteLine($"Please addapt your file based on the following example");
                ImportSource sampleSite =
                        new ImportSource
                        {
                            Id = 12345,
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
                        };
                Console.WriteLine(JsonConvert.SerializeObject(new[] { sampleSite }, Formatting.Indented));
            }
        }

        private async Task ImportSources(HttpClient client, JToken sources, int? siteId = null)
        {
            var sourceArray = sources.ToObject<JArray>();
            foreach (var sourceToken in sourceArray)
            {
                var source = sourceToken.ToObject<ImportSource>();

                if (source.Id.HasValue)
                {
                    var sourcePatch = new JsonPatchDocument();
                    WalkNode(sourceToken, "", (n, path) =>
                    {
                        foreach (var property in n.Properties())
                        {
                            if (property.Value.Type != JTokenType.Array && property.Value.Type != JTokenType.Object && property.Value.Type != JTokenType.Property
                                && !string.Equals(property.Name, nameof(ImportSource.Id), StringComparison.InvariantCultureIgnoreCase))
                            {
                                sourcePatch.Replace($"{path}{property.Name}", property.ToObject(GetPropertyType(property.Value.Type)));
                            }
                        }
                    });
                    await DataCreator.PatchSource(client, source.Id.Value, sourcePatch);
                    Console.WriteLine($"Updated source - id <{source.Id}> named <{source.Name}>");
                }
                else
                {
                    if (siteId.HasValue)
                    {
                        source.SiteId = siteId.Value;
                    }
                    source.Id = (await DataCreator.CreateSource(client, source)).Id;
                    Console.WriteLine($"Created source - id <{source.Id}> named <{source.Name}> in site <{source.SiteId}>");
                }
                await ImportVariables(sourceToken, client, source);
            }
        }

        private async Task ImportVariables(JToken sourceToken, HttpClient client, ImportSource source)
        {
            var variables = sourceToken[nameof(ImportSource.Variables)];
            if (variables != null)
            {
                var variableArray = variables.ToObject<JArray>();
                foreach (var variableToken in variableArray)
                {
                    var importVariable = variableToken.ToObject<VariableImport>();

                    if (importVariable.Id.HasValue)
                    {
                        var variablePatch = new JsonPatchDocument();
                        WalkNode(variableToken, "", (n, path) =>
                        {
                            foreach (var property in n.Properties())
                            {
                                if (property.Value.Type != JTokenType.Array && property.Value.Type != JTokenType.Object && property.Value.Type != JTokenType.Property
                                    && !string.Equals(property.Name, nameof(VariableImport.Id), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    variablePatch.Replace($"{path}{property.Name}", property.ToObject(GetPropertyType(property.Value.Type)));
                                }
                            }
                        });
                        await DataCreator.PatchVariable(client, source.Id.Value, importVariable.Id.Value, variablePatch);
                        Console.WriteLine($"update variable - id <{importVariable.Id}> of type <{importVariable.VariableTypeId}> source <{source.Name}>");
                    }
                    else
                    {
                        var variable = await DataCreator.CreateVariable(client, source.Id.Value, importVariable);
                        Console.WriteLine($"Created variable - id <{variable.Id}> of type <{importVariable.VariableTypeId}> source <{source.Name}>");
                    }
                }
            }
        }

        private static string LoadFile()
        {
            do
            {
                try
                {
                    Console.WriteLine("Please enter the .json file containing the sites and sources");
                    var readLine = Console.ReadLine();
                    var fileName = readLine.Trim().Trim('"');

                    if (!File.Exists(fileName))
                    {
                        Console.WriteLine($"Could not open the file <{fileName}>");
                    }
                    else
                    {
                        Console.WriteLine($"Opening file {fileName}");
                        var json = File.ReadAllText(fileName);
                        return json;

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine();
                    Console.WriteLine($"FAILED to open file, please retry...");
                    Console.WriteLine();
                }

            } while (true);
        }

        private Type GetPropertyType(JTokenType propertyType)
        {
            switch (propertyType)
            {
                case JTokenType.Integer:
                    return typeof(int);
                case JTokenType.Float:
                    return typeof(double);
                case JTokenType.Uri:
                case JTokenType.String:
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return typeof(string);
                case JTokenType.Boolean:
                    return typeof(bool);
                case JTokenType.Date:
                    return typeof(DateTime);
                case JTokenType.Raw:
                case JTokenType.Bytes:
                    return typeof(byte[]);
                case JTokenType.Guid:
                    return typeof(Guid);
                case JTokenType.TimeSpan:
                    return typeof(TimeSpan);
                default:
                    return typeof(string);
            }
        }

        static void WalkNode(JToken node, string path, Action<JObject, string> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node, path);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    var subPath = $"{path}{child.Name}/";
                    if (subPath.Contains(nameof(ImportSite.ClientData)))
                    {
                        WalkNode(child.Value, subPath, action);
                    }
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                var children = node.Children().ToList();
                for (int i = 0; i < children.Count; i++)
                {
                    WalkNode(children[i], $"{path}[{i}]/", action);
                }
            }
        }
    }

    //internal class DynamicAndCaseInsensitivePropertyPathResolver : CaseInsensitivePropertyPathResolver
    //{
    //    override 
    //}

    public class ImportSite
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? TypeId { get; set; }
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
        public int? SiteId { get; set; }
        public int? Id { get; set; }
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
        public int? Id { get; set; }
        public int VariableTypeId { get; set; }
        public int UnitId { get; set; }
        public int Divider { get; set; }
        public double Granularity { get; set; }
        public TimePeriod GranularityTimeBase { get; set; }
    }
}
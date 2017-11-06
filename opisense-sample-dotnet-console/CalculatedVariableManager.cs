using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    internal class CalculatedVariableManager
    {
        private readonly Authenticator authenticator;
        private readonly SourceSelector sourceSelector;
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];

        public CalculatedVariableManager(Authenticator authenticator, SourceSelector sourceSelector)
        {
            this.authenticator = authenticator;
            this.sourceSelector = sourceSelector;
        }

        public async Task CrudCalculatedVariables()
        {
            var result = ShowMenu();
            switch (result)
            {
                case 1:
                    await GetVariablesBySourceId();
                    break;
                case 2:
                    await Update();
                    break;
            }

        }

        private async Task Update()
        {
            Console.WriteLine("Enter source id");
            var sourceId = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Enter variable id");
            var variableId = Convert.ToInt32(Console.ReadLine());
            var client = await authenticator.GetAuthenticatedClient();
            var response = await client.GetAsync($"{OpisenseApi}variables?displayLevel=verbose&id={variableId}");
            response.EnsureSuccessStatusCode();

            var calculatedVariable = (await response.Content.ReadAsAsync<CalculatedVariable[]>()).FirstOrDefault();
            if (calculatedVariable != null)
            {
                var updateResponse = await client.PutAsJsonAsync($"{OpisenseApi}sources/{sourceId}/variables/{variableId}", calculatedVariable);
                updateResponse.EnsureSuccessStatusCode();
            }

        }

        private async Task GetVariablesBySourceId()
        {
            Console.WriteLine("Enter source id");
            var sourceId = Convert.ToInt32(Console.ReadLine());

            var client = await authenticator.GetAuthenticatedClient();
            var response = await client.GetAsync($"{OpisenseApi}variables?displayLevel=verbose&sourceId={sourceId}");
            response.EnsureSuccessStatusCode();

            var calculatedVariables = await response.Content.ReadAsAsync<CalculatedVariable[]>();

            Console.WriteLine(JsonConvert.SerializeObject(calculatedVariables, Formatting.Indented));

        }

        private int ShowMenu()
        {
            Console.WriteLine("Calculated variables:");
            Console.WriteLine("1. Display for sourceId");
            Console.WriteLine("2. Update");
            var result = Convert.ToInt32(Console.ReadLine());

            return result;
        }
    }

    internal class CalculatedVariable : Variable
    {
        public Calculated Calculated { get; set; }
    }

    public class Calculated
    {

        /// <summary>
        /// Id = impacting VariableId
        /// </summary>
        public int Id { get; set; }
        public string FriendlyName { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

        public List<CalculatedVariableFormula> CalculatedVariableFormulas { get; set; } = new List<CalculatedVariableFormula>();
    }

    public class CalculatedVariableFormula
    {
        public string Formula { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public List<CalculatedVariableDependencyVariable> Variables { get; set; } = new List<CalculatedVariableDependencyVariable>();
        public List<CalculatedVariableDependencyEntity> Entities { get; set; } = new List<CalculatedVariableDependencyEntity>();
        public List<CalculatedVariableDependencyConstant> Constants { get; set; } = new List<CalculatedVariableDependencyConstant>();
    }

    public class CalculatedVariableDependencyConstant
    {

        public double Value { get; set; }
        public string Alias { get; set; }
    }
    public class CalculatedVariableDependencyVariable
    {

        public string Alias { get; set; }
        public int SiteId { get; set; }
        public int SourceId { get; set; }
        public int VariableId { get; set; }

        public Granularity Granularity { get; set; }
        public Aggregate? Aggregation { get; set; }

        public PeriodType PeriodType { get; set; }
        public double Period { get; set; }
        public TimePeriod PeriodTimeBase { get; set; }
        public DateTime? FromAbsolutePeriod { get; set; }
        public DateTime? ToAbsolutePeriod { get; set; }
        public int UnitId { get; set; }
        public bool CanBeUsedAsATrigger { get; set; }
    }

    public enum PeriodType
    {
        RelativeDate = 1,
        AbsoluteDate = 2,
        LastNPoints = 3
    }

    public class CalculatedVariableDependencyEntity
    {


        public string Alias { get; set; }
        public EntityType EntityType { get; set; }
        public int SiteId { get; set; }
        public int SourceId { get; set; }
        public int VariableId { get; set; }
        public string FormId { get; set; }
        public int GroupId { get; set; }
        public string PropId { get; set; }

        public int EntityId
        {
            get
            {
                switch (EntityType)
                {
                    case EntityType.Site:
                        return SiteId;
                    case EntityType.Source:
                        return SourceId;
                    default: return 0;
                }
            }
        }
    }

    public enum Granularity
    {
        Raw = 0,
        Minute = 1,
        Hour = 2,
        Day = 3,
        Week = 7,
        Month = 4,
        Year = 5,
        All = 6
    }
    public enum Aggregate
    {
        SUM = 1,
        MIN = 2,
        MAX = 3,
        AVG = 4,
        COUNT = 5,
        VAR = 6,
        STDEV = 7
    }
    public enum EntityType
    {
        Site = 1,
        Source = 2,
    }

}
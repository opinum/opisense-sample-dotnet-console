namespace opisense_sample_dotnet_console.Model
{
    public class Variable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SourceId { get; set; }
        public int VariableTypeId { get; set; }
        public int UnitId { get; set; }
        public double Granularity { get; set; }
        public TimePeriod GranularityTimeBase { get; set; }
        public string MappingConfig { get; set; }
        public QuantityType QuantityType { get; set; }
    }

    public enum QuantityType
    {
        Instantaneous = 1,
        Integrated = 2,
        Cumulative = 3,
        Minimum = 4,
        Maximum = 5,
        Averaged = 6
    }
}
namespace opisense_sample_dotnet_console.Model
{
    public class Variable
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int VariableTypeId { get; set; }
        public int UnitId { get; set; }
        public int Divider { get; set; }
        public double Granularity { get; set; }
        public TimePeriod GranularityTimeBase { get; set; }
    }
}
using System;

namespace opisense_sample_dotnet_console.Model
{
    public class ValueData
    {
        public int VariableId { get; set; }
        public DateTime Date { get; set; }
        public double? RawValue { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? CountValue { get; set; }
        public double? AvgValue { get; set; }
        public double? StdevValue { get; set; }
        public double? VarianceValue { get; set; }
        public double? SumValue { get; set; }

        public double? GetValue()
        {
            return RawValue ?? MinValue ?? MaxValue ?? CountValue ?? AvgValue ?? StdevValue ?? VarianceValue ?? SumValue;
        }
    }
}
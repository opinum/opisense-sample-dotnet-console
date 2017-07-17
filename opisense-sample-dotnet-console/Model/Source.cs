namespace opisense_sample_dotnet_console.Model
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public dynamic ClientData { get; set; }
    }
}
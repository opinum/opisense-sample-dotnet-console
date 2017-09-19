using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace opisense_sample_dotnet_console
{
    public class StorageLoader
    {
        private readonly Authenticator authenticator;
        private static readonly string OpisenseApi = ConfigurationManager.AppSettings["OpisenseApi"];

        public StorageLoader(Authenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public async Task LoadFileFromString(string fileName, string fileContent, int? siteId = null)
        {
            var byteArray = Encoding.ASCII.GetBytes(fileContent);
            using (var stream = new MemoryStream(byteArray))
            {
                await LoadFileFromStream(fileName, stream, siteId);
            }
        }

        public async Task LoadFileFromStream(string fileName, Stream stream, int? siteId = null)
        {
            using (var client = await authenticator.GetAuthenticatedClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    content.Add(new StreamContent(stream), fileName, fileName);

                    using (var response = await client.PostAsync($"{OpisenseApi}storage?parameters.filename={fileName}&parameters.siteId={siteId}", content))
                    {
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine();
                        Console.WriteLine("File successfully uploaded...");
                    }
                }
            }
        }
    }
}
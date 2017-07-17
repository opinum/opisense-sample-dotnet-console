using System;
using System.Collections.Generic;
using opisense_sample_dotnet_console.Model;

namespace opisense_sample_dotnet_console
{
    internal class Program
    {
        private static void Main()
        {
            var authenticator = new Authenticator();
            var siteSelector = new SiteSelector();
            var sourceSelector = new SourceSelector(siteSelector, authenticator);
            var variableSelector = new VariableSelector(sourceSelector);
            var dataCreator = new DataCreator(variableSelector, authenticator);
            var reporter = new Reporter(variableSelector, authenticator);
            var siteDeletor = new SiteDeletor(siteSelector, authenticator);
            var sourceDeletor = new SourceDeletor(sourceSelector, authenticator);
            var importer = new Importer(authenticator);
            var exitCode = 99;
            int userInput;
            do
            {
                userInput = DisplayMenu(exitCode);
                try
                {
                    switch (userInput)
                    {
                        case 1:
                            sourceSelector.DisplaySources().Wait();
                            break;
                        case 2:
                            dataCreator.DemoSetup().Wait();
                            break;
                        case 3:
                            dataCreator.UpdateData().Wait();
                            break;
                        case 4:
                            reporter.DisplayData().Wait();
                            break;
                        case 5:
                            siteDeletor.DeleteSite().Wait();
                            break;
                        case 6:
                            sourceDeletor.DeleteSource().Wait();
                            break;
                        case 7:
                            importer.ImportDefinitions().Wait();
                            break;
                        case 8:
                            sourceSelector.SearchSources().Wait();
                            break;
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                    Console.WriteLine("Something bad happened. Please try again...");
                }

            } while (userInput != exitCode);
        }

        private static int DisplayMenu(int exitCode)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Opisense Integration Demo");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            Console.WriteLine("1. List all sources on account");
            Console.WriteLine("2. Demo Setup");
            Console.WriteLine("3. Update data");
            Console.WriteLine("4. View Data");
            Console.WriteLine("5. Delete a site (WARNING: UNRECOVERABLE)");
            Console.WriteLine("6. Delete a source (WARNING: UNRECOVERABLE)");
            Console.WriteLine("7. Import sites and sources (using JSON File)");
            Console.WriteLine("8. Search sources (using Custom filter)");

            Console.WriteLine();
            Console.WriteLine("-------------------------");
            Console.WriteLine($"{exitCode}. Exit");
            var result = Console.ReadLine();
            return Convert.ToInt32(result);
        }
    }

  
}

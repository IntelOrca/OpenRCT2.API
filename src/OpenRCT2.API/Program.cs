using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OpenRCT2.API
{
    public class Program
    {
        private const int DefaultPort = 5004;
        private const string ConfigDirectory = ".openrct2";
        private const string ConfigFileName = "api.config.json";

        public static int Main(string[] args)
        {
            PrintArguments(args);

            string bindAddress = $"http://localhost:{DefaultPort}";
            if (args.Length >= 2)
            {
                bindAddress = args[1];
            }

            var configuration = BuildConfiguration();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(bindAddress)
                .UseConfiguration(configuration)
                .Build();

            host.Run();
            return 0;
        }

        private static void PrintArguments(string[] args)
        {
            Console.WriteLine("Starting OpenRCT2.API with arguments:");
            foreach (string arg in args)
            {
                Console.WriteLine("  " + arg);
            }
            Console.WriteLine("---------------");
        }

        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();

            string configDirectory = GetConfigDirectory();
            if (Directory.Exists(configDirectory))
            {
                builder
                    .SetBasePath(configDirectory)
                    .AddJsonFile(ConfigFileName, optional: true, reloadOnChange: true);
            }

            builder.AddEnvironmentVariables();
            return builder.Build();
        }

        private static string GetConfigDirectory()
        {
            string homeDirectory = Environment.GetEnvironmentVariable("HOME");
            if (String.IsNullOrEmpty(homeDirectory))
            {
                homeDirectory = Environment.GetEnvironmentVariable("HOMEDRIVE") +
                                Environment.GetEnvironmentVariable("HOMEPATH");
                if (String.IsNullOrEmpty(homeDirectory))
                {
                    homeDirectory = "~";
                }
            }
            string configDirectory = Path.Combine(homeDirectory, ConfigDirectory);
            return configDirectory;
        }
    }
}

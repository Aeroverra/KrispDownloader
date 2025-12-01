using Aeroverra.KrispDownloader.Configuration;
using Aeroverra.KrispDownloader.Services;
using Serilog;
using Newtonsoft.Json;

namespace Aeroverra.KrispDownloader
{
    public class Program
    {
        public const string ConsoleTitle = "Krisp Downloader By Aeroverra (Nicholas Halka)";
        public static async Task  Main(string[] args)
        {
            Console.Title = ConsoleTitle;
            if ((await EnsureAppSettingsExists()) == false)
            {
                return;
            }

            var builder = Host.CreateApplicationBuilder(args);

            SerilogConfig.ConfigureBootstrapLogger();
            builder.Services.AddSerilog(SerilogConfig.ConfigureFullLogger);

            // Configure services
            builder.Services.Configure<KrispApiConfiguration>(builder.Configuration.GetSection("KrispApi"));

            builder.Services.AddHttpClient<KrispApiService>();
            builder.Services.AddSingleton<FileService>();
            builder.Services.AddSingleton<TranscriptParsingService>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }

        private static async Task<bool> EnsureAppSettingsExists()
        {
            const string fileName = "appsettings.json";
            if (File.Exists(fileName))
            {
                return true;
            }

            var defaultConfig = new KrispApiConfiguration
            {
                BaseUrl = "https://api.krisp.ai",
                BearerToken = "your-bearer-token-here"
            };

            var appSettings = new AppSettings { KrispApi = defaultConfig };

            var json = JsonConvert.SerializeObject(appSettings, Formatting.Indented);

            File.WriteAllText(fileName, json);
            Console.WriteLine($"Created default '{fileName}'. Please update it with your settings and run again.");
            await Task.Delay(20000);
            return false;
        }
    }

}

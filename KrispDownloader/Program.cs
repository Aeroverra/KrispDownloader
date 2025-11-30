using KrispDownloader.Configuration;
using KrispDownloader.Services;
using Serilog;

namespace KrispDownloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
    }
}
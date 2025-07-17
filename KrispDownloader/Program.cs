using KrispDownloader.Configuration;
using KrispDownloader.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KrispDownloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            
            // Configure services
            builder.Services.Configure<KrispApiConfiguration>(
                builder.Configuration.GetSection("KrispApi"));
            
            builder.Services.AddHttpClient<KrispApiService>();
            builder.Services.AddSingleton<FileService>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
    }
}
}
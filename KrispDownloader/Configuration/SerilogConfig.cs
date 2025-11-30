using Serilog;
using Serilog.Events;

namespace KrispDownloader.Configuration
{
    public class SerilogConfig
    {
        public static void ConfigureBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
        }

        public static void ConfigureFullLogger(IServiceProvider serviceProvider, LoggerConfiguration loggerConfiguration)
        {
            // Debug serilog internal errors
            //SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog] {msg}"));

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            loggerConfiguration.ReadFrom.Configuration(configuration)
                        .ReadFrom.Services(serviceProvider)
                        .Enrich.FromLogContext()
                        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)// httpclient request spam
                        .WriteTo.Console();
            //.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"); // output context to find spam source

            var connectionString = configuration.GetConnectionString("DefaultConnection");
        }
    }
}

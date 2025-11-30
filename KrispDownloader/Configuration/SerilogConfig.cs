using Serilog;
using Serilog.Debugging;
using System.Collections.ObjectModel;
using System.Data;

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
                        .WriteTo.Console();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
        }
    }
}

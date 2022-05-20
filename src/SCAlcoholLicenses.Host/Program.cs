using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Domain;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace SCAlcoholLicenses.Host
{
	class Program
	{
		public static async Task Main(string[] args)
        {
            using var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<App>();

                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("App"));

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Singleton);

                    services.AddSingleton<LicenseService>();

                    services.AddSingleton(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<LicenseClient>>();
                        var settings = provider.GetRequiredService<IOptions<AppSettings>>();
                        return new LicenseClient(logger, settings.Value.Proxy.Hostname, settings.Value.Proxy.Username,
                            settings.Value.Proxy.Password);
                    });
                })
                .Build();

            await host.Services.GetService<App>()!.Run();
            await host.StopAsync();
        }

	}
}

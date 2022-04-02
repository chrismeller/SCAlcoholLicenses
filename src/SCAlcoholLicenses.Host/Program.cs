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


namespace SCAlcoholLicenses.Host
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			var services = new ServiceCollection();
			ConfigureServices(services);

			var serviceProvider = services.BuildServiceProvider();

			await serviceProvider.GetService<App>()!.Run();
		}

		private static void ConfigureServices(IServiceCollection services)
        {
			services.AddLogging(builder =>
			{
				builder.AddConsole();
				builder.AddDebug();
			});

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
				.Build();

			services.Configure<AppSettings>(configuration.GetSection("App"));

			services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
			services.AddTransient((provider) => provider.GetService<ApplicationDbContext>()!.GetDbConnection());

			services.AddTransient<LicenseService>();

            services.AddTransient((provider) =>
            {
                var logger = provider.GetRequiredService<ILogger<LicenseClient>>();
                var settings = provider.GetRequiredService<IOptions<AppSettings>>();
                return new LicenseClient(logger, settings.Value.Proxy.Hostname, settings.Value.Proxy.Username, settings.Value.Proxy.Password);
            });

            services.AddTransient<App>();
        }

	}
}

using Claims.Auditing;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Audit.Startup))]
namespace Audit
{
    public class Startup : FunctionsStartup
    {

        private static IConfiguration _configuration = null;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("Values:QueueConnectionString");

            var serviceProvider = builder.Services.BuildServiceProvider();
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();

            builder.Services.AddDbContext<AuditContext>(options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"local.settings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }
    }
}

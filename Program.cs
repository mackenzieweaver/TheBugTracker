using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace TheBugTracker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await ProgramaticMigration(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static async Task ProgramaticMigration(IHost host)
        {
            try
            {
                using var svcScope = host.Services.CreateScope();
                var svcProvider = svcScope.ServiceProvider;
                var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
                await dbContextSvc.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while running Programatic Migration Data => {ex}");
            }
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheBugTracker.Data;
using TheBugTracker.Hubs;
using TheBugTracker.Models;
using TheBugTracker.Services;
using TheBugTracker.Services.Interfaces;
using Npgsql;
using System;
using TheBugTracker.Services.Factories;

namespace TheBugTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        public string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(dbOptions =>
                dbOptions.UseNpgsql(GetConnectionString(Configuration),
                sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            ));
            
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<BTUserClaimsPrincipalFactory>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();
                
            services.AddScoped<IBTRolesService, BTRolesService>();
            services.AddScoped<IBTCompanyInfoService, BTCompanyInfoService>();
            services.AddScoped<IBTProjectService, BTProjectService>();
            services.AddScoped<IBTTicketService, BTTicketService>();
            services.AddScoped<IBTTicketHistoryService, BTTicketHistoryService>();
            services.AddScoped<IBTNotificationService, BTNotificationService>();
            services.AddScoped<IBTInviteService, BTInviteService>();
            services.AddScoped<IBTFileService, BTFileService>();
            services.AddScoped<ISeedService, SeedService>();
            services.AddScoped<IBTLookupService, BTLookupService>();
            
            services.AddScoped<IEmailSender, BTEmailService>();
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            
            services.AddControllersWithViews();

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<CallHub>("/callHub");
            });
        }
    }
}

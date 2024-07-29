using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoPro24Team06.Data;
using SoPro24Team06.Models;
using Xunit;

namespace SoPro24Team06.E2E
{
    public class TestFixtures : IDisposable
    {
        public readonly string BaseUrl = "https://localhost:7003/";
        public ServiceProvider ServiceProvider { get; private set; }
        public ApplicationDbContext Context { get; private set; }
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<ApplicationRole> RoleManager { get; private set; }

        public TestFixtures()
        {
            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("DataSource=:memory:")
            );
            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            ServiceProvider = services.BuildServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.OpenConnection();
                Context = context;
                context.Database.EnsureCreated();

                UserManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                RoleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();
            }
        }

        public void Dispose()
        {
            Context.Database.CloseConnection();
            ServiceProvider.Dispose();
        }
    }
}


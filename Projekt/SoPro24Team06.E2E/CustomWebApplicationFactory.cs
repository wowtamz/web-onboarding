using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.E2E
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        private readonly string _baseAddress = "https://localhost:7003";
        private IHost _webHost;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(_baseAddress);
            builder.ConfigureServices(services =>
            {
                // get old Database Connection
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                );
                // if foun remove old Database Connection
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                //add new Db Discriptor
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase")
                );
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var dummyHost = builder.Build();
            //builder.ConfigureWebHost();
            builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel();
            });
            _webHost = builder.Build();
            _webHost.Start();

            return dummyHost;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webHost?.StopAsync().GetAwaiter().GetResult();
                _webHost?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

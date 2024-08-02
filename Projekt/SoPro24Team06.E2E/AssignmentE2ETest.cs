using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoPro24Team06;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

namespace SoPro24Team06.E2E
{
    public class AssignmentE2ETest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AssignmentE2ETest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TestIfIsRunning()
        {
            var client = new WebApplicationFactory<Program>().CreateClient();
            bool isRunning = false;
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    var response = await client.GetAsync("https://localhost:7003");
                    if (response.IsSuccessStatusCode)
                    {
                        isRunning = true;
                        break;
                    }
                }
                catch { }
                await Task.Delay(10000);
            }
            if (isRunning == false)
            {
                throw new Exception("client not running");
            }
        }
    }
}

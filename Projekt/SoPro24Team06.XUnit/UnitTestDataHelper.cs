using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SoPro24Team06;
using SoPro24Team06.Data;

namespace SoPro24Team06.XUnit
{
    public static class UnitTestDataHelper
    {
        public static async Task<ApplicationDbContext> CreateDbInMomory()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return context;
        }
    }
}

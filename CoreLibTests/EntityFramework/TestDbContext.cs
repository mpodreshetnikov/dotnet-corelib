using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CoreLibTests.EntityFramework
{
    internal class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    }

    internal class TestDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public static TestDbContext CreateContext()
        {
            var context = new TestDbContext();
            context.Database.EnsureCreated();
            return context;
        }

        private SqliteConnection connection = new("Filename=:memory:");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            connection.Open();
            optionsBuilder.UseSqlite(connection);
        }

        public override void Dispose()
        {
            connection.Dispose();
            base.Dispose();
        }
    }
}


using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CoreLibTests.EntityFramework
{
    internal class User
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; } = default!;
        public virtual Company? Company { get; set; }

        public static User Generate(Faker faker = null!)
        {
            faker ??= new Faker();
            return new User
            {
                FullName = faker.Name.FullName(),
                Company = Company.Generate(faker),
            };
        }

        public static IEnumerable<User> Generate(int count, Faker faker = null!)
        {
            faker ??= new Faker();
            return faker.MakeLazy(count, () => Generate(faker));
        }
    }

    internal class Company
    {
        public Guid Id { get; set; }

        public string? Name { get; set; } = default!;

        public static Company Generate(Faker faker = null!)
        {
            faker ??= new Faker();
            return new Company
            {
                Name = faker.Company.CompanyName(),
            };
        }

        public static IEnumerable<Company> Generate(int count, Faker faker = null!)
        {
            faker ??= new Faker();
            return faker.MakeLazy(count, () => Generate(faker));
        }
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


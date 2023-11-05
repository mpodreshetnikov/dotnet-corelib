using CoreLib.EntityFramework.Features.Encryption;
using CoreLib.EntityFramework.Features.Encryption.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CoreLibTests.EntityFramework.Features.Encryption;

public class PropertyBuilderExtensionsTests
{
    #region Preparations

    internal class EncryptedTestDbContext : TestDbContext
    {
        public ICryptoConverter? CryptoConverter { get; set; }

        public int EncryptedPropertyMaxLength { get; set; } = 20;

        public Type EncryptedMigration { get; set; } = null!;

        public EncryptedTestDbContext()
        {
        }

        public EncryptedTestDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }

        public static EncryptedTestDbContext CreateContext(
            ICryptoConverter? cryptoConverter = default!,
            int encryptedPropertyMaxLength = 20,
            Type encryptedMigration = default!)
        {
            var context = new EncryptedTestDbContext()
            {
                CryptoConverter = cryptoConverter,
                EncryptedPropertyMaxLength = encryptedPropertyMaxLength,
                EncryptedMigration = encryptedMigration,
            };
            context.Database.EnsureCreated();
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .EncryptedWith(CryptoConverter!, EncryptedPropertyMaxLength, EncryptedMigration);
        }
    }

    #endregion

    private async Task<EncryptedTestDbContext> SeedDbContext(
        ICryptoConverter cryptoConverter = null!,
        IEnumerable<User> users = null!,
        int? maxLength = null!)
    {
        var cryptKey = new Faker().Internet.Password(16);
        var authKey = new Faker().Internet.Password();
        cryptoConverter ??= new DefaultCryptoConverter(cryptKey, authKey);

        var dbContext = EncryptedTestDbContext.CreateContext(
            cryptoConverter,
            maxLength ?? 20);

        var totalItems = 100;
        users ??= User.Generate(totalItems).ToList();
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        return dbContext;
    }

    [Fact]
    public async Task EncryptedWith_EncryptProperty_ValuesCanBeReadFromDbContext()
    {
        // Arrange
        var users = User.Generate(100).ToList();
        var dbContext = await SeedDbContext(users: users);

        var expectedUser = users.First();
        var user = await dbContext.Users.SingleAsync(x => x.Id == expectedUser.Id);

        // Assert
        user.Password.Should().Be(expectedUser.Password);
    }

    [Fact]
    public async Task EncryptedWith_EncryptProperty_ValuesCannotBeReadableFromDatabaseBySQL()
    {
        // Arrange
        var cryptKey = new Faker().Internet.Password(16);
        var authKey = new Faker().Internet.Password();
        var cryptoConverter = new DefaultCryptoConverter(cryptKey, authKey);

        var users = User.Generate(100).ToList();
        var dbContext = await SeedDbContext(cryptoConverter: cryptoConverter, users: users);

        var expectedUser = users.First();
        var rawPassword = await dbContext.Database.SqlQuery<string>($"SELECT [Password] as [Value] FROM [Users] WHERE [Id] = {expectedUser.Id}").SingleAsync();
        var decryptedPassword = cryptoConverter.Decrypt(rawPassword);

        // Assert
        rawPassword.Should().NotBe(expectedUser.Password);
        decryptedPassword.Should().Be(expectedUser.Password);
    }

    [Fact]
    public async Task EncryptedWith_EncryptProperty_OtherPropertiesNotEncrypted()
    {
        // Arrange
        var users = User.Generate(100).ToList();
        var dbContext = await SeedDbContext(users: users);

        var expectedUser = users.First();
        var rawFullname = await dbContext.Database.SqlQuery<string>($"SELECT [FullName] as [Value] FROM [Users] WHERE [Id] = {expectedUser.Id}").SingleAsync();

        // Assert
        rawFullname.Should().Be(expectedUser.FullName);
    }

    [Fact]
    public async Task EncryptedWith_EncryptPropertyAndUseDifferentKeysToDecrypt_ThrowsException()
    {
        // Arrange
        var cryptKey = new Faker().Internet.Password(16);
        var authKey = new Faker().Internet.Password();
        var anotherCryptoConverter = new DefaultCryptoConverter(cryptKey, authKey);

        var users = User.Generate(100).ToList();
        // use here default crypto converter
        var dbContext = await SeedDbContext(users: users);

        var expectedUser = users.First();
        var rawPassword = await dbContext.Database.SqlQuery<string>($"SELECT [Password] as [Value] FROM [Users] WHERE [Id] = {expectedUser.Id}").SingleAsync();

        // Assert
        rawPassword.Should().NotBe(expectedUser.Password);
        FluentActions.Invoking(() => anotherCryptoConverter.Decrypt(rawPassword))
            .Should().Throw<DecryptionException>();
    }
}
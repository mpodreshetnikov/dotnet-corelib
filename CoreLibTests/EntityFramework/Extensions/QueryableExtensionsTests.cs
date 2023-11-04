using CoreLib.EntityFramework.Extensions;
using CoreLibTests.EntityFramework;

namespace EfCoreExtensionTests.Utils.EntityFramework.Extensions;

public class QueryableExtensionsTests
{
    #region SelectOrDefault

    [Fact]
    public async Task SelectOrDefault_NotMemberAccessorExpression_ThrowsException()
    {
        // Arrange
        var totalItems = 100;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        // Assert
        FluentActions.Invoking(() =>
            query.SelectOrDefault(user => user.FullName + "test"))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task SelectOrDefault_AccessQueryWithNullMember_ReturnsDefaultValueWhereNull()
    {
        // Arrange
        var users = User.Generate(3).ToList();
        users.ElementAt(1).Company = null;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        // default value where company is null
        var expected = new string[] { users.ElementAt(0).Company!.Name, default!, users.ElementAt(2).Company!.Name };

        // Action
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var actual1 = query.SelectOrDefault(person => person.Company.Name);
#pragma warning restore CS8602
        var actual2 = query.SelectOrDefault(person => person.Company!.Name);

        // Assert
        actual1.Should().BeEquivalentTo(expected);
        actual2.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task SelectOrDefault_AccessQueryWithNullMemberAndSetDefaultValueManual_ReturnsDefaultValueWhereNull()
    {
        // Arrange
        var users = User.Generate(3).ToList();
        users.ElementAt(1).Company = null;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var _default = "default";

        // default value where company is null
        var expected = new string[] { users.ElementAt(0).Company!.Name, _default, users.ElementAt(2).Company!.Name };

        // Action
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var actual1 = query.SelectOrDefault(person => person.Company.Name, _default);
#pragma warning restore CS8602
        var actual2 = query.SelectOrDefault(person => person.Company!.Name, _default);

        // Assert
        actual1.Should().BeEquivalentTo(expected);
        actual2.Should().BeEquivalentTo(expected);
    }

    #endregion
}
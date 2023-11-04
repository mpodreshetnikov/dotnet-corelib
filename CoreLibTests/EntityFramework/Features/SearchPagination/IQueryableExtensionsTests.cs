using CoreLib.EntityFramework.Features.SearchPagination;
using Microsoft.EntityFrameworkCore;

namespace CoreLibTests.EntityFramework.Features.SearchPagination;

public class IQueryableExtensionsTests
{
    #region ApplyPagination

    [Fact]
    public void ApplyPagination_NullQuery_NotPaginated()
    {
        // Arrange
        var query = new Faker().Make(10, () => new Person()).AsQueryable();
        var expected = query;
        // Action
        var actual = query.ApplyPagination(null!);
        // Assert
        actual.Should().Equal(expected);
    }

    [Fact]
    public void ApplyPagination_OnlyOffset_ItemsSkipped()
    {
        // Arrange
        var query = new Faker().Make(100, () => new Person()).AsQueryable();

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(() => null!);
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(1, 50));
        var pagedQuery = mock.Object;

        var expected = query.Skip(pagedQuery.Offset!.Value);

        // Action
        var actual = query.ApplyPagination(pagedQuery);
        // Assert
        actual.Should().Equal(expected);
    }

    [Fact]
    public void ApplyPagination_OnlyLimit_ItemsTook()
    {
        // Arrange
        var query = new Faker().Make(100, () => new Person()).AsQueryable();

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(new Faker().Random.Int(0, 50));
        mock.Setup(x => x.Offset).Returns(() => null!);
        var pagedQuery = mock.Object;

        var expected = query.Take(pagedQuery.Limit!.Value);

        // Action
        var actual = query.ApplyPagination(pagedQuery);
        // Assert
        actual.Should().Equal(expected);
    }

    [Fact]
    public void ApplyPagination_LimitAndOffset_ItemsSkippedAndTook()
    {
        // Arrange
        var query = new Faker().Make(100, () => new Person()).AsQueryable();

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(new Faker().Random.Int(0, 50));
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(0, 50));
        var pagedQuery = mock.Object;

        var expected = query
            .Skip(pagedQuery.Offset!.Value)
            .Take(pagedQuery.Limit!.Value);

        // Action
        var actual = query.ApplyPagination(pagedQuery);
        // Assert
        actual.Should().Equal(expected);
    }

    #endregion

    #region AsPagedResultAsync

    [Fact]
    public async Task AsPagedResultAsync_NullPagedQuery_ThrowsException()
    {
        // Arrange
        var query = new Faker().Make(10, () => new Person()).AsQueryable();
        // Assert
        await FluentActions.Invoking(() => query.AsPagedResultAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AsPagedResultAsync_WithApplyPagination_CorrectPagedResult()
    {
        // Arrange
        var totalItems = 100;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(
                new Faker<User>()
                    .RuleFor(x => x.Name, faker => faker.Name.FullName())
                    .Generate(totalItems));
        await dbContext.SaveChangesAsync();

        var query = dbContext.Users;
        
        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(new Faker().Random.Int(0, 50));
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(0, 50));
        var pagedQuery = mock.Object;

        var expectedItems = query
            .Skip(pagedQuery.Offset!.Value)
            .Take(pagedQuery.Limit!.Value);

        var expected = new PagedResult<User>()
        {
            Items = await expectedItems.ToListAsync(),
            ItemsOffset = pagedQuery.Offset!.Value,
            ItemsQuantity = await expectedItems.CountAsync(),
            TotalItems = totalItems,
        };

        // Action
        var actual = await query.AsPagedResultAsync(pagedQuery);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AsPagedResultAsync_WithoutApplyPaginationAndWithoutTotalItemsRewrite_Throws()
    {

    }

    [Fact]
    public void AsPagedResultAsync_WithoutApplyPagination_CorrectPagedResult()
    {

    }

    #endregion
}
using CoreLib.EntityFramework.Features.SearchPagination;
using Microsoft.EntityFrameworkCore;

namespace CoreLibTests.EntityFramework.Features.SearchPagination;

public class QueryableExtensionsTests
{
    #region ApplyPagination

    [Fact]
    public async Task ApplyPagination_NullQuery_NotPaginated()
    {
        // Arrange
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(100));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var expected = query;
        // Action
        var actual = query.ApplyPagination(null!);
        // Assert
        actual.Should().Equal(expected);
    }

    [Fact]
    public async Task ApplyPagination_QueryNotOrdered_ThrowsException()
    {
        // Arrange
        var totalItems = 100;
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(() => null!);
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(1, 50));
        var pagedQuery = mock.Object;

        // Assert
        FluentActions.Invoking(() => query.ApplyPagination(pagedQuery))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ApplyPagination_OnlyOffset_ItemsSkipped()
    {
        // Arrange
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(100));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

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
    public async Task ApplyPagination_OnlyLimit_ItemsTook()
    {
        // Arrange
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(100));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

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
    public async Task ApplyPagination_LimitAndOffset_ItemsSkippedAndTook()
    {
        // Arrange
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(100));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

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
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(100));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);
        // Assert
        await FluentActions.Invoking(async () => await query.AsPagedResultAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AsPagedResultAsync_QueryNotOrdered_ThrowsException()
    {
        // Arrange
        var totalItems = 100;
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(() => null!);
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(1, 50));
        var pagedQuery = mock.Object;

        // Assert
        await FluentActions.Invoking(() => query.AsPagedResultAsync(pagedQuery))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AsPagedResultAsync_WithApplyPagination_CorrectPagedResult()
    {
        // Arrange
        var totalItems = 100;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

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
        actual.Items.Should().Equal(expectedItems);
    }

    [Fact]
    public async Task AsPagedResultAsync_WithoutApplyPaginationAndWithoutTotalItemsRewrite_Throws()
    {
        // Arrange
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(100));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(new Faker().Random.Int(0, 50));
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(0, 50));
        var pagedQuery = mock.Object;

        // Assert
        await FluentActions.Invoking(async () =>
            await query.AsPagedResultAsync(pagedQuery, applyPagination: false, totalItemsRewrite: null))
           .Should().ThrowAsync<ArgumentNullException>();
        await FluentActions.Invoking(async () =>
            await query.AsPagedResultAsync(pagedQuery, applyPagination: false))
           .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AsPagedResultAsync_WithoutApplyPaginationAtAll_CorrectPagedResult()
    {
        var totalItems = 100;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

        var mock = new Mock<IPagedQuery>();
        mock.Setup(x => x.Limit).Returns(new Faker().Random.Int(0, 50));
        mock.Setup(x => x.Offset).Returns(new Faker().Random.Int(0, 50));
        var pagedQuery = mock.Object;

        var expectedItems = query;

        var expected = new PagedResult<User>()
        {
            Items = await expectedItems.ToListAsync(),
            ItemsOffset = pagedQuery.Offset!.Value,
            ItemsQuantity = await expectedItems.CountAsync(),
            TotalItems = totalItems,
        };

        // Action
        var actual = await query.AsPagedResultAsync(pagedQuery, applyPagination: false, totalItemsRewrite: totalItems);

        // Assert
        actual.Should().BeEquivalentTo(expected);
        actual.Items.Should().Equal(expectedItems);
    }

    [Fact]
    public async Task AsPagedResultAsync_WithoutApplyPaginationWithManualApplyPagination_CorrectPagedResult()
    {
        var totalItems = 100;

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users.OrderBy(x => x.FullName);

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
        var manuallyPaged = query.ApplyPagination(pagedQuery);
        var actual = await manuallyPaged.AsPagedResultAsync(pagedQuery, applyPagination: false, totalItemsRewrite: totalItems);

        // Assert
        actual.Should().BeEquivalentTo(expected);
        actual.Items.Should().Equal(expectedItems);
    }

    #endregion

    #region ApplySearch

    [Fact]
    public async Task ApplySearch_EmptyOrNullPropsToSearch_ThrowsException()
    {
        // Arrange
        var totalItems = 100;
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<ISearchQuery>();
        mock.Setup(x => x.SearchQuery).Returns("test");
        var searchQuery = mock.Object;

        // Assert
        FluentActions.Invoking(() => query.ApplySearch(searchQuery))
            .Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => query.ApplySearch(searchQuery, null!))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ApplySearch_NullOrEmptySearchQuery_NoEffect()
    {
        // Arrange
        var totalItems = 100;
        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(User.Generate(totalItems));
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<ISearchQuery>();
        mock.Setup(x => x.SearchQuery).Returns(string.Empty);
        var searchQuery = mock.Object;

        // Action
        var actual = query.ApplySearch(searchQuery, x => x.FullName);

        // Assert
        actual.Should().Equal(query);
    }

    [Fact]
    public async Task ApplySearch_OneSearchPropDifferentCasesTrailingSpaces_SuccessfulCaseInsensitiveSearch()
    {
        // Arrange
        var searchString = " Ab  ";
        var searchableUsers = new[]
        {
            new User
            {
                FullName = "ab",
            },
            new User
            {
                FullName = "Ab",
            },
            new User
            {
                FullName = "aBcd",
            },
            new User
            {
                FullName = "test cABInet!",
            },
        };
        var notSearchableUsers = new[]
        {
            new User
            {
                FullName = "a b",
            },
            new User
            {
                FullName = "Acb",
            },
            new User
            {
                FullName = string.Empty,
            },
            new User
            {
                FullName = "aPricos",
            },
            new User
            {
                FullName = null!,
            },
        };

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(searchableUsers);
        await dbContext.Users.AddRangeAsync(notSearchableUsers);
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<ISearchQuery>();
        mock.Setup(x => x.SearchQuery).Returns(searchString);
        var searchQuery = mock.Object;

        var expected = searchableUsers;

        // Action
        var actual = await query.ApplySearch(searchQuery, x => x.FullName).ToArrayAsync();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ApplySearch_TwoSearchProps_SuccessfulSearch()
    {
        // Arrange
        var searchString = "Wood";
        var searchableUsers = new[]
        {
            new User
            {
                FullName = "Woodpicker",
                Company = null,
            },
            new User
            {
                FullName = "Woodpicker",
                Company = new Company
                {
                    Name = null!,
                },
            },
            new User
            {
                FullName = "Donuld Wood",
                Company = new Company
                {
                    Name = "Wood Inc.",
                },
            },
            new User
            {
                FullName = " woodside street ",
                Company = new Company
                {
                    Name = "Nothing Company",
                },
            },
            new User
            {
                FullName = "Some guy",
                Company = new Company
                {
                    Name = "Wood Inc.",
                },
            },
            new User
            {
                FullName = null,
                Company = new Company
                {
                    Name = "Some wood Inc.",
                },
            },
        };
        var notSearchableUsers = new[]
        {
            new User
            {
                FullName = null,
                Company = null,
            },
            new User
            {
                FullName = "Some guy",
                Company = null,
            },
            new User
            {
                FullName = null,
                Company = new Company
                {
                    Name = null!,
                },
            },
            new User
            {
                FullName = null,
                Company = new Company
                {
                    Name = "Nothing Company",
                },
            },
            new User
            {
                FullName = "Some super guy",
                Company = new Company
                {
                    Name = "Nothing Company",
                },
            },
        };

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(searchableUsers);
        await dbContext.Users.AddRangeAsync(notSearchableUsers);
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<ISearchQuery>();
        mock.Setup(x => x.SearchQuery).Returns(searchString);
        var searchQuery = mock.Object;

        var expected = searchableUsers;

        // Action
        var actual1 = await query.ApplySearch(searchQuery, x => x.FullName, x => x.Company!.Name).ToArrayAsync();
        var actual2 = await query.ApplySearch(searchQuery, x => x.Company!.Name, x => x.FullName).ToArrayAsync();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var actual3 = await query.ApplySearch(searchQuery, x => x.Company.Name, x => x.FullName).ToArrayAsync();
#pragma warning restore CS8602

        // Assert
        actual1.Should().BeEquivalentTo(expected);
        actual2.Should().BeEquivalentTo(expected);
        actual3.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ApplySearch_ChildNullableProp_SuccessfulSearch()
    {
        // Arrange
        var searchString = "Wood";
        var searchableUsers = new[]
        {
            new User
            {
                Company = new Company
                {
                    Name = "Wood Inc.",
                },
            },
        };
        var notSearchableUsers = new[]
        {
            new User
            {
                FullName = null,
                Company = null,
            },
        };

        var dbContext = TestDbContext.CreateContext();
        await dbContext.Users.AddRangeAsync(searchableUsers);
        await dbContext.Users.AddRangeAsync(notSearchableUsers);
        await dbContext.SaveChangesAsync();
        var query = dbContext.Users;

        var mock = new Mock<ISearchQuery>();
        mock.Setup(x => x.SearchQuery).Returns(searchString);
        var searchQuery = mock.Object;

        var expected = searchableUsers;

        // Action
        var actual = await query.ApplySearch(searchQuery, x => x.Company!.Name).ToArrayAsync();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    #endregion
}
using CoreLib.Utils;
using FluentAssertions;

namespace EfCoreExtensionTests.Utils;

public class ReflectionUtilsTests
{
    #region IsQueryOrdered

    [Fact]
    public void IsQueryOrdered_QueryOrderedDifferentMethods_True()
    {
        // Arrange
        var query = Enumerable.Range(0, 10).AsQueryable();
        var orderedQuery_Order = query.Order();
        var orderedQuery_OrderDescending = query.OrderDescending();
        var orderedQuery_OrderBy = query.OrderBy(x => x);
        var orderedQuery_OrderByDescending = query.OrderByDescending(x => x);
        // Action
        var orderedQuery_Order_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_Order);
        var orderedQuery_OrderDescending_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_OrderDescending);
        var orderedQuery_OrderBy_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_OrderBy);
        var orderedQuery_OrderByDescending_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_OrderByDescending);
        // Assert
        orderedQuery_Order_isOrdered.Should().BeTrue();
        orderedQuery_OrderBy_isOrdered.Should().BeTrue();
        orderedQuery_OrderDescending_isOrdered.Should().BeTrue();
        orderedQuery_OrderByDescending_isOrdered.Should().BeTrue();
    }

    [Fact]
    public void IsQueryOrdered_QueryOrderedDifferentMethodsWithPostAction_True()
    {
        // Arrange
        var query = Enumerable.Range(0, 10).AsQueryable();
        var orderedQuery_Order = query.Order().Select(x => x);
        var orderedQuery_OrderDescending = query.OrderDescending().Select(x => x);
        var orderedQuery_OrderBy = query.OrderBy(x => x).Select(x => x);
        var orderedQuery_OrderByDescending = query.OrderByDescending(x => x).Select(x => x);
        // Action
        var orderedQuery_Order_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_Order);
        var orderedQuery_OrderDescending_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_OrderDescending);
        var orderedQuery_OrderBy_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_OrderBy);
        var orderedQuery_OrderByDescending_isOrdered = ReflectionUtils.IsQueryOrdered(orderedQuery_OrderByDescending);
        // Assert
        orderedQuery_Order_isOrdered.Should().BeTrue();
        orderedQuery_OrderBy_isOrdered.Should().BeTrue();
        orderedQuery_OrderDescending_isOrdered.Should().BeTrue();
        orderedQuery_OrderByDescending_isOrdered.Should().BeTrue();
    }

    [Fact]
    public void IsQueryOrdered_QueryNotOrdered_False()
    {
        // Arrange
        var query = Enumerable.Range(0, 10).AsQueryable();
        // Action
        var orderedQuery_isOrdered = ReflectionUtils.IsQueryOrdered(query);
        // Assert
        orderedQuery_isOrdered.Should().BeFalse();
    }

    #endregion
}
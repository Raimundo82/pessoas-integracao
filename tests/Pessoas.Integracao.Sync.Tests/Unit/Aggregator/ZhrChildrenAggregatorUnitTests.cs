using FluentAssertions;

using Pessoas.Integracao.Sync.Infrastructure.Services.Aggregator;
using Pessoas.Integracao.Sync.Tests.Unit.Helpers;

namespace Pessoas.Integracao.Sync.Tests.Unit.Aggregator;

public class ZhrChildrenAggregatorTests
{
    [Fact]
    public void Aggregate_ShouldReturnEmpty_WhenOutputsAreEmpty()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        // Act
        var result = aggregator.Aggregate([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_ShouldReturnSingleGroup_WhenAllChildrenHaveSameType()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<ZhrTestOutput>
        {
            new()
            {
                Ni = "NI1",
                Numsap = "NUMSAP1",
                Children = [
                    new ZhrChildA { Ni = "NI1" },
                    new ZhrChildA { Ni = "NI1" }
                ]
            },
            new()
            {
                Ni = "NI2",
                Numsap = "NUMSAP2",
                Children = [new ZhrChildA { Ni = "NI2" }]
            }
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
        result[0].Should().OnlyContain(x => x is ZhrChildA);
    }

    [Fact]
    public void ShouldGroupChildrenByConcreteType_WhenMultipleChildTypesArePresent()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<ZhrTestOutput>
        {
            new()
            {
                Ni = "NI1",
                Numsap = "NUMSAP1",
                Children = [new ZhrChildA { Ni = "NI1" }]
            },
            new()
            {
                Ni = "NI2",
                Numsap = "NUMSAP2",
                Children = [new ZhrChildB { Ni = "NI2" }]
            },
            new()
            {
                Ni = "NI3",
                Numsap = "NUMSAP3",
                Children = [new ZhrChildA { Ni = "NI3" }]
            }
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.Should().HaveCount(2);

        result.Should()
                .Contain(x => x.All(c => c is ZhrChildA) && x.Length == 2);

        result.Should()
            .Contain(x => x.All(c => c is ZhrChildB) && x.Length == 1);
    }

    [Fact]
    public void ShouldFlattenChildren_WhenMultipleOutputsAreProvided()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<ZhrTestOutput>
        {
            new()
            {
                Ni = "NI1",
                Numsap = "NUMSAP1",
                Children = [new ZhrChildA { Ni = "NI1" }]
            },
            new()
            {
                Ni = "NI2",
                Numsap = "NUMSAP2",
                Children = [
                    new ZhrChildA { Ni = "NI2" },
                    new ZhrChildA { Ni = "NI2" }
                ]
            },
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
    }

    [Fact]
    public void ShouldIgnoreOutputs_WhenOutputsHaveNoChildren()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<ZhrTestOutput>
        {
            new() {Ni = "NI", Numsap = "NUMSAP"},
            new()
            {
                Ni = "NI1",
                Numsap = "NUMSAP1",
                Children = [new ZhrChildA { Ni = "NI1" }]
            },
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().ContainSingle();
    }

    [Fact]
    public void ShouldPreserveAllChildren_WhenMultipleChildrenAreProvided()
    {
        // Arrange
        var child1 = new ZhrChildA { Ni = "NI1" };
        var child2 = new ZhrChildA { Ni = "NI2" };
        var child3 = new ZhrChildB() { Ni = "NI3" };

        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<ZhrTestOutput>
        {
            new() {
                Ni = "NI",
                Numsap = "NUMSAP",
                Children =
                [
                    child1,
                    child2,
                    child3
                ]
            }
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.SelectMany(x => x)
              .Should()
              .Contain([child1, child2, child3]);
    }
}

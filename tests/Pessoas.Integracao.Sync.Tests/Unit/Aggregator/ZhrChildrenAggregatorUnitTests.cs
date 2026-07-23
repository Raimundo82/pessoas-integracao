using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Infrastructure.Services.Aggregator;

namespace Pessoas.Integracao.Sync.Tests.Unit.Aggregator;

public class ZhrChildrenAggregatorTests
{
    private sealed class TestOutput : ZhrSBaseModelOutput
    {
        public IReadOnlyList<ZhrSBaseModel> Children { get; init; } = [];

        public override IReadOnlyList<ZhrSBaseModel> GetChildren() => Children;
    }

    private sealed class ChildA : ZhrSBaseModel { }

    private sealed class ChildB : ZhrSBaseModel { }

    [Fact]
    public void Aggregate_ShouldReturnEmpty_WhenOutputsAreEmpty()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        // Act
        var result = aggregator.Aggregate<TestOutput>([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_ShouldReturnSingleGroup_WhenAllChildrenHaveSameType()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<TestOutput>
        {
            new() {
                Children =
                [
                    new ChildA { Ni = "NI1" },
                    new ChildA { Ni = "NI2" }
                ]
            },
            new() {
                Children =
                [
                    new ChildA { Ni = "NI3" }
                ]
            }
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
        result[0].Should().OnlyContain(x => x is ChildA);
    }

    [Fact]
    public void ShouldGroupChildrenByConcreteType_WhenMultipleChildTypesArePresent()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<TestOutput>
        {
            new() {
                Children =
                [
                    new ChildA { Ni = "NI1" },
                    new ChildB { Ni = "NI2" },
                    new ChildA { Ni = "NI3" }
                ]
            }
        };

        // Act
        var result = aggregator.Aggregate(outputs);

        // Assert
        result.Should().HaveCount(2);

        result.Should()
            .Contain(x => x.All(c => c is ChildA) && x.Length == 2);

        result.Should()
            .Contain(x => x.All(c => c is ChildB) && x.Length == 1);
    }

    [Fact]
    public void ShouldFlattenChildren_WhenMultipleOutputsAreProvided()
    {
        // Arrange
        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<TestOutput>
        {
            new() {
                Children =
                [
                    new ChildA { Ni = "NI1" },
                ]
            },
            new() {
                Children =
                [
                    new ChildA { Ni = "NI2" },
                    new ChildA { Ni = "NI3" },
                ]
            }
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

        var outputs = new List<TestOutput>
        {
            new(),
            new() {
                Children =
                [
                    new ChildA { Ni = "NI1" },
                ]
            }
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
        var child1 = new ChildA { Ni = "NI1" };
        var child2 = new ChildA { Ni = "NI2" };
        var child3 = new ChildB() { Ni = "NI3" };

        var aggregator = new ZhrChildrenAggregator();

        var outputs = new List<TestOutput>
        {
            new() {
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

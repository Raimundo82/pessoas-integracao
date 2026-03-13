using FluentAssertions;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;

namespace Pessoas.Integracao.Worker.Tests.Unit.Correlator;

public sealed class SoapResultCorrelatorUnitTests
{
    private readonly SoapResultCorrelator _sut = new();

    [Fact]
    public void ShouldReturnEmptyDictionary_WhenKeysArrayIsEmpty()
    {
        // Arrange
        var keys = Array.Empty<PessoaImportKey>();
        var output = new[] { Item("A", "one") };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnKeyMappedToOutputItem_WhenSingleKeyAndSingleMatchingOutput()
    {
        // Arrange
        var keys = new[] { Key("A") };
        var expected = Item("A", "one");
        var output = new[] { expected };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().HaveCount(1);
        result[Key("A")].Should().BeSameAs(expected);
    }


    [Fact]
    public void ShouldReturnMultipleKeysWithMatches_WhenAllKeysHaveMatches()
    {
        // Arrange
        var keys = new[] { Key("A"), Key("B") };
        var first = Item("A", "one");
        var second = Item("B", "two");
        var output = new[] { first, second };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().HaveCount(2);
        result[Key("A")].Should().BeSameAs(first);
        result[Key("B")].Should().BeSameAs(second);
    }

    [Fact]
    public void ShouldReturnKeyMappedToNull_WhenKeyHasNoMatch()
    {
        // Arrange
        var keys = new[] { Key("A") };
        var output = new[] { Item("B", "two") };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().HaveCount(1);
        result[Key("A")].Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullForAllKeys_WhenOutputIsNull()
    {
        // Arrange
        var keys = new[] { Key("A"), Key("B") };

        // Act
        var result = _sut.CorrelateByKey<TestOutput>(keys, null, x => x.Nii!);

        // Assert
        result.Should().HaveCount(2);
        result[Key("A")].Should().BeNull();
        result[Key("B")].Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullForAllKeys_WhenOutputIsEmpty()
    {
        // Arrange
        var keys = new[] { Key("A"), Key("B") };

        // Act
        var result = _sut.CorrelateByKey(keys, Array.Empty<TestOutput>(), x => x.Nii!);

        // Assert
        result.Should().HaveCount(2);
        result[Key("A")].Should().BeNull();
        result[Key("B")].Should().BeNull();
    }

    [Fact]
    public void ShouldPartiallyMatchMultipleKeys_WhenSomeKeysHaveMatches()
    {
        // Arrange
        var keys = new[] { Key("A"), Key("B"), Key("C") };
        var first = Item("A", "one");
        var third = Item("C", "three");
        var output = new[] { first, third };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().HaveCount(3);
        result[Key("A")].Should().BeSameAs(first);
        result[Key("B")].Should().BeNull();
        result[Key("C")].Should().BeSameAs(third);
    }

    [Fact]
    public void ShouldFilterOutOutputItems_WhenSelectorReturnsNullOrWhitespace()
    {
        // Arrange
        var keys = new[] { Key("A") };
        var output = new[]
        {
            Item(null, "null-nii"),
            Item(" ", "whitespace-nii"),
            Item("A", "valid")
        };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().HaveCount(1);
        result[Key("A")]!.Value.Should().Be("valid");
    }

    [Fact]
    public void ShouldKeepFirstOutputItem_WhenMultipleOutputItemsHaveSameNii()
    {
        // Arrange
        var keys = new[] { Key("A") };
        var first = Item("A", "first");
        var second = Item("A", "second");
        var output = new[] { first, second };

        // Act
        var result = _sut.CorrelateByKey(keys, output, x => x.Nii!);

        // Assert
        result.Should().HaveCount(1);
        result[Key("A")].Should().BeSameAs(first);
    }

    private static PessoaImportKey Key(string nii)
        => new(nii, null);

    private static TestOutput Item(string? nii, string value)
        => new() { Nii = nii, Value = value };

    private sealed class TestOutput
    {
        public string? Nii { get; init; }
        public string Value { get; init; } = string.Empty;
    }
}
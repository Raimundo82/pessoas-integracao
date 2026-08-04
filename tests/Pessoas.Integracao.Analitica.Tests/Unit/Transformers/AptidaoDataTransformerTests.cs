using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.Transformers;

public sealed class AptidaoDataTransformerTests
{
    private readonly Mock<IEntityMapper<ZhrWsAptidaoAptidao>> _mapper = new();
    private readonly AptidaoDataTransformer _transformer;

    public AptidaoDataTransformerTests()
    {
        _transformer = new AptidaoDataTransformer(new AptidaoMapper());
    }

    [Fact]
    public void ShouldReturnEmptyCollection_WhenZhrOutputsIsEmpty()
    {
        // Arrange
        var zhrOutputs = new List<IZhrOutput>();

        // Act
        var result = _transformer.Transform(zhrOutputs).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyCollection_WhenAptidoesIsNullOrEmpty()
    {
        // Arrange
        var zhrOutputs = new List<IZhrOutput>
        {
            ZhrOutputTestData.OutputWith(aptidoes: null),
            ZhrOutputTestData.OutputWith(aptidoes: [])
        };

        // Act
        var result = _transformer.Transform(zhrOutputs).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldMapAndEnrichEachAptidaoItem_WhenValidDataProvided()
    {
        // Arrange
        var zhrItem = new ZhrSAptidao { Ni = "123", Subty = "0001" };
        var analiticaItem = new ZhrWsAptidaoAptidao { Ni = "123", Subty = "0001" };

        var timestamp = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var zhrOutput = ZhrOutputTestData.OutputWith(
            externalId: "3000",
            updateAt: timestamp,
            aptidoes: [zhrItem]
        );

        var zhrOutputs = new List<IZhrOutput> { zhrOutput };

        _mapper.Setup(m => m.Map(zhrItem)).Returns(analiticaItem);

        // Act
        var result = _transformer.Transform(zhrOutputs).ToList();

        // Assert
        // Assert
        result.Should().HaveCount(1);
        var analiticaItemResult = result.First();
        analiticaItemResult.UpdatedAt.Should().Be(timestamp);
        analiticaItemResult.Numsap.Should().Be("3000");
        analiticaItemResult.Ni.Should().Be("123");
        analiticaItemResult.Subty.Should().Be("0001");
    }

    [Fact]
    public void ShouldCombineAptidoesFromMultipleInputs()
    {
        // Arrange
        var item11 = new ZhrSAptidao { Ni = "1", Subty = "0001" };
        var item12 = new ZhrSAptidao { Ni = "1", Subty = "0001" };
        var item2 = new ZhrSAptidao { Ni = "2", Subty = "0002" };

        var output1 = ZhrOutputTestData.OutputWith(ni: "1", aptidoes: [item11, item12]);
        var output2 = ZhrOutputTestData.OutputWith(ni: "2", aptidoes: [item2]);
        var outputs = new List<IZhrOutput> { output1, output2 };

        _mapper.Setup(m => m.Map(item11)).Returns(new ZhrWsAptidaoAptidao { Ni = "1", Subty = "0001" });
        _mapper.Setup(m => m.Map(item12)).Returns(new ZhrWsAptidaoAptidao { Ni = "1", Subty = "0001" });
        _mapper.Setup(m => m.Map(item2)).Returns(new ZhrWsAptidaoAptidao { Ni = "2", Subty = "0002" });

        // Act
        var result = _transformer.Transform(outputs).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Select(r => r.Ni).Should().Contain(["1", "1", "2"]);
    }
}

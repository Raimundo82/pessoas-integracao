using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.Transformers;

public sealed class PessoaisDataTransformerTests
{
    private readonly Mock<IEntityMapper<ZhrWsPersonalDataPessoai>> _mapper = new();
    private readonly PessoaisDataTransformer _transformer;

    public PessoaisDataTransformerTests()
    {
        _transformer = new PessoaisDataTransformer(new PessoaisMapper());
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
    public void ShouldReturnEmptyCollection_WhenPessoaisIsNullOrEmpty()
    {
        // Arrange
        var zhrOutputs = new List<IZhrOutput>
        {
            ZhrOutputTestData.OutputWith(pessoais: null),
            ZhrOutputTestData.OutputWith(pessoais: [])
        };

        // Act
        var result = _transformer.Transform(zhrOutputs).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldMapAndEnrichEachPessoaisItem_WhenValidDataProvided()
    {
        // Arrange
        var zhrItem = new ZhrSPessoais { Ni = "123", Apelido = "Silva" };
        var analiticaItem = new ZhrWsPersonalDataPessoai { Ni = "123", Apelido = "Silva" };

        var timestamp = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var zhrOutput = ZhrOutputTestData.OutputWith(
            externalId: "3000",
            updateAt: timestamp,
            pessoais: [zhrItem]
        );

        var zhrOutputs = new List<IZhrOutput> { zhrOutput };

        _mapper.Setup(m => m.Map(zhrItem)).Returns(analiticaItem);

        // Act
        var result = _transformer.Transform(zhrOutputs).ToList();

        // Assert
        result.Should().HaveCount(1);
        var analiticaItemResult = result.First();
        analiticaItemResult.UpdatedAt.Should().Be(timestamp);
        analiticaItemResult.Numsap.Should().Be("3000");
        analiticaItemResult.Ni.Should().Be("123");
        analiticaItemResult.Apelido.Should().Be("Silva");
    }

    [Fact]
    public void ShouldCombinePessoaisFromMultipleInputs()
    {
        // Arrange
        var item1 = new ZhrSPessoais { Ni = "1", Apelido = "Santos" };
        var item2 = new ZhrSPessoais { Ni = "2", Apelido = "Silva" };

        var output1 = ZhrOutputTestData.OutputWith(ni: "1", pessoais: [item1]);
        var output2 = ZhrOutputTestData.OutputWith(ni: "2", pessoais: [item2]);
        var outputs = new List<IZhrOutput> { output1, output2 };

        _mapper.Setup(m => m.Map(item1)).Returns(new ZhrWsPersonalDataPessoai { Ni = "1", Apelido = "Santos" });
        _mapper.Setup(m => m.Map(item2)).Returns(new ZhrWsPersonalDataPessoai { Ni = "2", Apelido = "Silva" });

        // Act
        var result = _transformer.Transform(outputs).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Ni).Should().Contain(["1", "2"]);
    }
}

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.AnaliticaSynchronizer;

public sealed class AptidaoSynchronizerTests
{
    private readonly Mock<IEntityMapper<ZhrWsAptidaoAptidao>> _mapper = new();
    private readonly Mock<IAnaliticaRepository<ZhrWsAptidaoAptidao>> _repository = new();

    [Fact]
    public async Task ShouldNotCallMapperOrRepository_WhenAptidoesIsNull()
    {
        // Arrange
        var input = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: null) };
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        _mapper.Invocations.Should().BeEmpty();
        _repository.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldNotCallMapperOrRepository_WhenAptidoesIsEmpty()
    {
        // Arrange
        var input = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: []) };
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        _repository.Invocations.Should().BeEmpty();
        _mapper.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldMapEachItem_WhenExternalIdIsProvidedFromZhrOutput()
    {
        // Arrange
        var item = new ZhrSAptidao { Ni = "1" };
        var mapped = new ZhrWsAptidaoAptidao { Ni = "1", Numsap = "3000" };
        var input = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(externalId: "3000", aptidoes: [item]) };

        _mapper.Setup(m => m.Map(item)).Returns(mapped);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        _mapper.Verify(m => m.Map(item), Times.Once);
        mapped.Numsap.Should().Be("3000");
        mapped.Ni.Should().Be("1");
    }

    [Fact]
    public async Task ShouldMapAllItems_WhenAptidoesHasMultipleEntries()
    {
        // Arrange
        var item1 = new ZhrSAptidao { Ni = "1", Subty = "0001" };
        var item2 = new ZhrSAptidao { Ni = "1", Subty = "0002" };

        var input = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: [item1, item2]) };

        _mapper.Setup(m => m.Map(It.IsAny<ZhrSAptidao>()))
            .Returns((ZhrSAptidao s) => new ZhrWsAptidaoAptidao { Ni = s.Ni, Subty = s.Subty });

        IReadOnlyList<ZhrWsAptidaoAptidao>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsAptidaoAptidao>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsAptidaoAptidao>, CancellationToken>((list, _) => captured = list);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured.Should().HaveCount(2);

        captured.Select(c => c.Subty).Should().Contain(["0001", "0002"]);
    }

    [Fact]
    public async Task ShouldPersistFullyPopulatedAptidao_WhenSourceIsFullyPopulated()
    {
        // Arrange
        var source = new ZhrSAptidao
        {
            Id = 999,
            Ni = "20002",
            Subty = "0001",
            Denominacao = "Aptidão Física",
            AreaExame = "Cardiologia",
            ArexamesDesc = "Exame Cardiológico",
            ServicoMedInt = "Serviço Médico Interno",
            Valor = 18.5m,
            DataExame = "2026-01-15",
            Modalidade = "Presencial",
            ModalDesc = "Exame presencial",
            Resultado = "Apto",
            ResultadoDesc = "Apto para o serviço",
            Observacoes = "Sem observações relevantes"
        };

        var input = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(externalId: "3000", aptidoes: [source]) };

        IReadOnlyList<ZhrWsAptidaoAptidao>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsAptidaoAptidao>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsAptidaoAptidao>, CancellationToken>((list, _) => captured = list)
            .Returns(Task.CompletedTask);

        var sut = new AptidaoSynchronizer(new AptidaoMapper(), _repository.Object);

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        await Verify(captured);
    }

    [Fact]
    public async Task ShouldCombineCollectionsFromMultipleUsers_WhenMultipleInputsProvided()
    {
        // Arrange
        var item1 = new ZhrSAptidao { Ni = "1", Subty = "0001" };
        var item2 = new ZhrSAptidao { Ni = "2", Subty = "0002" };

        var input1 = ZhrOutputTestData.OutputWith(ni: "1", aptidoes: [item1]);
        var input2 = ZhrOutputTestData.OutputWith(ni: "2", aptidoes: [item2]);
        var inputs = new List<IZhrOutput> { input1, input2 };

        _mapper
            .Setup(m => m.Map(It.IsAny<ZhrSAptidao>()))
            .Returns((ZhrSAptidao s) => new ZhrWsAptidaoAptidao { Ni = s.Ni, Subty = s.Subty });

        IReadOnlyList<ZhrWsAptidaoAptidao>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsAptidaoAptidao>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsAptidaoAptidao>, CancellationToken>((list, _) => captured = list);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(inputs, CancellationToken.None);

        // Assert
        _repository.Verify(
            r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsAptidaoAptidao>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        captured.Should().NotBeNull();
        captured.Should().HaveCount(2);
        captured.Select(c => c.Ni).Should().Contain(["1", "2"]);
    }

    [Fact]
    public async Task ShouldEnrichEachItemWithItsOwnUserData_WhenMultipleInputsProvided()
    {
        // Arrange
        var item1 = new ZhrSAptidao { Ni = "1" };
        var item2 = new ZhrSAptidao { Ni = "2" };

        var input1 = ZhrOutputTestData.OutputWith(ni: "1", externalId: "3000", aptidoes: [item1]);
        var input2 = ZhrOutputTestData.OutputWith(ni: "2", externalId: "4000", aptidoes: [item2]);
        var inputs = new List<IZhrOutput> { input1, input2 };

        _mapper.Setup(m => m.Map(item1)).Returns(new ZhrWsAptidaoAptidao { Ni = "1" });
        _mapper.Setup(m => m.Map(item2)).Returns(new ZhrWsAptidaoAptidao { Ni = "2" });

        IReadOnlyList<ZhrWsAptidaoAptidao>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsAptidaoAptidao>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsAptidaoAptidao>, CancellationToken>((list, _) => captured = list);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(inputs, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.Single(c => c.Ni == "1").Numsap.Should().Be("3000");
        captured!.Single(c => c.Ni == "2").Numsap.Should().Be("4000");
    }

    [Fact]
    public async Task ShouldOnlyIncludeItemsFromUsersWithData_WhenSomeInputsHaveNullOrEmptyCollections()
    {
        // Arrange
        var item = new ZhrSAptidao { Ni = "2" };

        var input1 = ZhrOutputTestData.OutputWith(ni: "1", aptidoes: null);
        var input2 = ZhrOutputTestData.OutputWith(ni: "2", aptidoes: [item]);
        var input3 = ZhrOutputTestData.OutputWith(ni: "3", aptidoes: []);
        var inputs = new List<IZhrOutput> { input1, input2, input3 };

        _mapper.Setup(m => m.Map(item)).Returns(new ZhrWsAptidaoAptidao { Ni = "2" });

        IReadOnlyList<ZhrWsAptidaoAptidao>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsAptidaoAptidao>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsAptidaoAptidao>, CancellationToken>((list, _) => captured = list);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(inputs, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured.Should().ContainSingle().Which.Ni.Should().Be("2");
    }

    [Fact]
    public async Task ShouldNotCallMapperOrRepository_WhenInputsListIsEmpty()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(new List<IZhrOutput>(), CancellationToken.None);

        // Assert
        _mapper.Invocations.Should().BeEmpty();
        _repository.Invocations.Should().BeEmpty();
    }

    private AptidaoSynchronizer CreateSut() => new(_mapper.Object, _repository.Object);

}

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.AnaliticaSynchronizer;

public sealed class PessoaisSynchronizerTests
{
    private readonly Mock<IEntityMapper<ZhrWsPersonalDataPessoai>> _mapper = new();
    private readonly Mock<IAnaliticaRepository<ZhrWsPersonalDataPessoai>> _repository = new();

    [Fact]
    public async Task ShouldNotCallMapperOrRepository_WhenPessoaisIsNull()
    {
        // Arrange
        var input = ZhrOutputTestData.OutputWith(pessoais: null);
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        _mapper.Invocations.Should().BeEmpty();
        _repository.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldNotCallMapperOrRepository_WhenPessoaisIsEmpty()
    {
        // Arrange
        var input = ZhrOutputTestData.OutputWith(pessoais: []);
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        _repository.Invocations.Should().BeEmpty();
        _mapper.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldMapEachItem_WhenExternalIdAndNumsapAreProvidedFromZhrOutput()
    {
        // Arrange
        var timestamp = new DateTimeOffset(new DateTime(2025, 1, 1));
        var item = new ZhrSPessoais { Ni = "1" };
        var mapped = new ZhrWsPersonalDataPessoai { Ni = "1", Numsap = "3000", };
        var input = ZhrOutputTestData
            .OutputWith(externalId: "3000", updateAt: timestamp, pessoais: [item]);

        _mapper.Setup(m => m.Map(item)).Returns(mapped);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        _mapper.Verify(m => m.Map(item), Times.Once);
        mapped.Numsap.Should().Be("3000");
        mapped.Ni.Should().Be("1");
        mapped.UpdatedAt.Should().BeExactly(timestamp);
    }

    [Fact]
    public async Task ShouldMapAllItems_WhenPessoaisHasMultipleEntries()
    {
        // Arrange
        var timestamp = new DateTimeOffset(new DateTime(2025, 1, 1, 15, 0, 30));
        var item1 = new ZhrSPessoais { Ni = "1", Apelido = "Toni" };
        var item2 = new ZhrSPessoais { Ni = "1", Apelido = "Bujo" };

        var input = ZhrOutputTestData.OutputWith("1", "3000", updateAt: timestamp, pessoais: [item1, item2]);

        _mapper.Setup(m => m.Map(It.IsAny<ZhrSPessoais>()))
            .Returns((ZhrSPessoais s) => new ZhrWsPersonalDataPessoai { Ni = s.Ni, Apelido = s.Apelido });

        IReadOnlyList<ZhrWsPersonalDataPessoai>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(
                It.IsAny<IReadOnlyList<ZhrWsPersonalDataPessoai>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsPersonalDataPessoai>, CancellationToken>((list, _) => captured = list);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured.Should().HaveCount(2);
        captured.Select(c => c.Apelido).Should().Contain(["Toni", "Bujo"]);
        captured.Select(c => c.Ni)
            .Distinct().Should().ContainSingle().Which.Should().Be("1");
        captured.Select(c => c.Numsap)
            .Distinct().Should().ContainSingle().Which.Should().Be("3000");
        captured.Select(c => c.UpdatedAt)
            .Distinct().Should().ContainSingle().Which.Should().BeExactly(timestamp);
    }

    [Fact]
    public async Task ShouldPersistFullyPopulatedPessoais_WhenSourceIsFullyPopulated()
    {
        // Arrange
        var timestamp = new DateTimeOffset(new DateTime(2025, 1, 1));
        var source = new ZhrSPessoais
        {
            Id = 999,
            Ni = "20002",
            Nome = "João Silva",
            Apelido = "Silva",
            Sexo = "M",
            SexoDesc = "Masculino",
            DtNasci = "19800101",
            Idade = "44",
            Idade31dezembro = "44",
            Nacionalidade1 = "107",
            Nacionalidade2 = "",
            Nacionalidade3 = "",
            PaisNat = "PT",
            PaisnascDesc = "PORTUGAL",
            DistritoNat = "15",
            DistnascDesc = "LISBOA",
            ConcelhoNat = "1503",
            ConcnascDesc = "LISBOA",
            FreguesiaNat = "150301",
            FregnascDesc = "ALCANTARA",
            EstCivil = "C",
            EstadocivilDesc = "Casado",
            DataEstCivil = "20050505",
            Rufnm = "JOAO",
            DtFalec = ""
        };

        var input = ZhrOutputTestData.OutputWith(
            externalId: "3000",
            updateAt: timestamp,
            pessoais: [source]
        );

        IReadOnlyList<ZhrWsPersonalDataPessoai>? captured = null;

        _repository
            .Setup(r => r.ReplaceMatchingByNiAsync(It.IsAny<IReadOnlyList<ZhrWsPersonalDataPessoai>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ZhrWsPersonalDataPessoai>, CancellationToken>((list, _) => captured = list)
            .Returns(Task.CompletedTask);

        var sut = new PessoaisSynchronizer(new PessoaisMapper(), _repository.Object);

        // Act
        await sut.SyncAsync(input, CancellationToken.None);

        // Assert
        await Verify(captured);
    }

    private PessoaisSynchronizer CreateSut() => new(_mapper.Object, _repository.Object);

}

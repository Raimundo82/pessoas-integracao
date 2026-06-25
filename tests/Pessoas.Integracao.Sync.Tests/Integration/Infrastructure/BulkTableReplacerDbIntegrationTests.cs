using FluentAssertions;

using Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;
using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class GraphReplacerDbIntegrationTests(PostgresTestContainerDb db) : TableReplacerTestsBase(db), IAsyncLifetime
{
    [Fact]
    public async Task ShouldInsertRootAndChildren_WhenDbIsEmpty()
    {

        // Arrange
        var newOutput = AptidaoOutput("22600", "30002697");
        newOutput.Aptidao = [AptidaoChild("22600", "Aptidao1"), AptidaoChild("22600", "Aptidao2")];

        // Act
        await ExecuteAsync([newOutput], [newOutput.Aptidao]);

        // Assert
        await using var assertContext = NewContext();
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(1);
        rootResult.Should().ContainSingle(r => r.Ni == "22600" && r.Numsap == "30002697" && r.UpdatedAt == null);
        childrenResult.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldWipeAllExistingDataAndInsertNewRoots_WhenDbIsPopulated()
    {

        // Arrange
        var (ni1, ni2) = ("22600", "226001");
        var updatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedAsync(
            AptidaoOutput(ni1, "30002697"),
           [AptidaoChild(ni1, "Aptidao1"), AptidaoChild(ni1, "Aptidao2")]);

        await SeedAsync(
            AptidaoOutput(ni2, "30002698"),
           [AptidaoChild(ni2, "Aptidao1"), AptidaoChild(ni2, "Aptidao2")]);

        await SeedAsync(
            new ZhrSPessoaisOutput { Ni = "226002", Numsap = "30002699" },
            [new ZhrSPessoais { Ni = "226002", Nome = "Pessoa2" }],
            [
                new ZhrSFamilia { Ni = "226002", Fcnam = "Familiar1" },
                new ZhrSFamilia { Ni = "226002", Fcnam = "Familiar2" }
            ]
        );

        var newAptidao = AptidaoOutput(ni1, "30002697", updatedAt);
        newAptidao.Aptidao = [AptidaoChild(ni1, "Aptidao3"), AptidaoChild(ni1, "Aptidao4")];

        var newPessoais = new ZhrSPessoaisOutput
        {
            Ni = "226002",
            Numsap = "30002699",
            Pessoais = [new ZhrSPessoais { Ni = "226002", Nome = "Pessoa2" }],
            Familia = [new ZhrSFamilia { Ni = "226002", Fcnam = "Familiar1" }]
        };


        // Act
        await ExecuteAsync([newAptidao], [newAptidao.Aptidao]);
        await ExecuteAsync([newPessoais], [newPessoais.Pessoais, newPessoais.Familia]);

        // Assert
        await using var assertContext = NewContext();

        var aptidaoRootResult = assertContext.Set<ZhrSAptidaoOutput>();
        aptidaoRootResult.Should().HaveCount(1);
        aptidaoRootResult.Should().ContainSingle(r => r.Ni == ni1 && r.Numsap == "30002697" && r.UpdatedAt == updatedAt);

        var childrenResult = assertContext.Set<ZhrSAptidao>();
        childrenResult.Should().HaveCount(2);
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao3");
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao4");

        var pessoaisRootResult = assertContext.Set<ZhrSPessoaisOutput>();
        pessoaisRootResult.Should().HaveCount(1);
        pessoaisRootResult.Should().ContainSingle(r => r.Ni == "226002" && r.Numsap == "30002699");

        var pessoaisResult = assertContext.Set<ZhrSPessoais>();
        pessoaisResult.Should().HaveCount(1);
        pessoaisResult.Should().ContainSingle(p => p.Ni == "226002" && p.Nome == "Pessoa2");

        var familiaResult = assertContext.Set<ZhrSFamilia>();
        familiaResult.Should().HaveCount(1);
        familiaResult.Should().ContainSingle(f => f.Ni == "226002" && f.Fcnam == "Familiar1");
    }


    [Fact]
    public async Task ShouldInsertMultipleRootsAndChildren_WhenDbIsEmpty()
    {
        // Arrange
        var (ni1, ni2) = ("22600", "22601");

        var outputs = new[]
        {
           new ZhrSAptidaoOutput { Ni = ni1, Numsap = "30002697", Aptidao = [AptidaoChild(ni1, "Aptidao1"), AptidaoChild(ni1, "Aptidao2")]},
           new ZhrSAptidaoOutput { Ni = ni2, Numsap = "30002698", Aptidao = [AptidaoChild(ni2, "Aptidao3"), AptidaoChild(ni2, "Aptidao4")]},
        };
        ZhrSAptidao[] aptidoes = [.. outputs.SelectMany(o => o.Aptidao)];

        // Act
        await ExecuteAsync(outputs, [aptidoes]);

        // Assert
        var assertContext = NewContext();
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        rootResult.Should().HaveCount(2);
        rootResult.Should().ContainSingle(r => r.Ni == ni1 && r.Numsap == "30002697");
        rootResult.Should().ContainSingle(r => r.Ni == ni2 && r.Numsap == "30002698");

        var childrenResult = assertContext.Set<ZhrSAptidao>();
        childrenResult.Should().HaveCount(4);
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao1");
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao2");
        childrenResult.Should().ContainSingle(c => c.Ni == ni2 && c.AreaExame == "Aptidao3");
        childrenResult.Should().ContainSingle(c => c.Ni == ni2 && c.AreaExame == "Aptidao4");
    }

    [Fact]
    public async Task ShouldWipeAllChildrenAndInsertNewOnes_WhenRootIsReplacedWithEmptyChildrenData()
    {

        // Arrange
        var ni = "22600";

        await SeedAsync(
            AptidaoOutput(ni, "30002697"), [AptidaoChild(ni, "Aptidao1"), AptidaoChild(ni, "Aptidao2")]);

        var replacement = AptidaoOutput(ni, "30002697");
        replacement.Aptidao = [];

        // Act
        await ExecuteAsync([replacement], [replacement.Aptidao]);

        // Assert
        await using var assertContext = NewContext();
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        rootResult.Should().HaveCount(1);

        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().ContainSingle(r => r.Ni == ni && r.Numsap == "30002697" && r.UpdatedAt == null);
        childrenResult.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPreserveOtherZhrOutputs_WhenOnlyOneZhrOutputIsReplaced()
    {
        // Arrange
        var (ni1, ni2) = ("22600", "226001");

        await SeedAsync(
            AptidaoOutput(ni1, "30002697"),
            [AptidaoChild(ni1, "Aptidao1"), AptidaoChild(ni1, "Aptidao2")]);

        await SeedAsync(
            AptidaoOutput(ni2, "30002698"),
            [AptidaoChild(ni2, "Aptidao3"), AptidaoChild(ni2, "Aptidao4")]);

        var updatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedAsync(
            new ZhrSPessoaisOutput { Ni = ni1, Numsap = "30002699" },
            [new ZhrSPessoais { Ni = ni1, Nome = "Pessoa2" }],
            [
                new ZhrSFamilia { Ni = ni1, Fcnam = "Familiar1" },
                new ZhrSFamilia { Ni = ni1, Fcnam = "Familiar2" }
            ]
        );

        var newAptidao = AptidaoOutput(ni1, "30002697", updatedAt);
        newAptidao.Aptidao = [AptidaoChild(ni1, "Aptidao5"), AptidaoChild(ni1, "Aptidao6")];

        await ExecuteAsync([newAptidao], [newAptidao.Aptidao]);

        // Assert
        await using var assertContext = NewContext();

        var aptidaoRootResult = assertContext.Set<ZhrSAptidaoOutput>();
        aptidaoRootResult.Should().HaveCount(1);
        aptidaoRootResult.Should().ContainSingle(r => r.Ni == ni1 && r.UpdatedAt == updatedAt);

        var aptidaoChildrenResult = assertContext.Set<ZhrSAptidao>();
        aptidaoChildrenResult.Should().HaveCount(2);
        aptidaoChildrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao5");
        aptidaoChildrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao6");

        var pessoaisRootResult = assertContext.Set<ZhrSPessoaisOutput>();
        pessoaisRootResult.Should().ContainSingle(c => c.Ni == ni1);

        var pessoaisChildrenResult = assertContext.Set<ZhrSPessoais>();
        pessoaisChildrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.Nome == "Pessoa2");

        var familiaChildrenResult = assertContext.Set<ZhrSFamilia>();
        familiaChildrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.Fcnam == "Familiar1");
        familiaChildrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.Fcnam == "Familiar2");
    }

    private async Task ExecuteAsync<TOutput>(TOutput[] outputs, IReadOnlyList<ZhrSBaseModel[]> children)
        where TOutput : ZhrSBaseModelOutput, IOutputModel
    {
        await using var context = NewContext();
        await new BulkTableReplacer(context).ExecuteAsync(outputs, children, _ct);
    }
}

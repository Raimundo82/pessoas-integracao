using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;
using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ZhrFetcherByNiDbIntegrationTests(PostgresTestContainerDb db) : TableReplacerTestsBase(db), IAsyncLifetime
{


    [Fact]
    public async Task ShouldReturnAllMatchingRows_WhenNisExistInTable()
    {
        // Arrange
        string ni1 = "0001", externalId1 = "30001";
        string ni2 = "0002", externalId2 = "30002";
        string ni3 = "0003", externalId3 = "30003";

        await SeedAsync(AptidaoOutput(ni1, externalId1), [AptidaoChild(ni1, "Aptidao1")]);
        await SeedAsync(AptidaoOutput(ni2, externalId2), [AptidaoChild(ni2, "Aptidao1")]);
        await SeedAsync(AptidaoOutput(ni3, externalId3), [AptidaoChild(ni3, "Aptidao1")]);

        var inputs = new List<PessoaSyncRef>
        {
            new() {Ni = ni1, ExternalId = externalId1},
            new() {Ni = ni2, ExternalId = externalId2},
            new() {Ni = ni3, ExternalId = externalId3},
        };

        // Act
        await using var actContext = NewContext();
        var result = await new ZhrFetcherByNi(actContext).ExecuteAsync<ZhrSAptidao>(inputs, _ct);

        // Assert
        result.Should().HaveCount(3);
        result.Where(i => i.Ni == ni1).Should().HaveCount(1);
        result.Where(i => i.Ni == ni2).Should().HaveCount(1);
        result.Where(i => i.Ni == ni3).Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenNoNisMatchInTable()
    {
        // Arrange
        string ni1 = "0001", externalId1 = "30001";
        string ni2 = "0002", externalId2 = "30002";

        await SeedAsync(AptidaoOutput(ni1, externalId1), [AptidaoChild(ni1, "Aptidao1")]);
        await SeedAsync(AptidaoOutput(ni2, externalId2), [AptidaoChild(ni2, "Aptidao1")]);

        var inputs = new List<PessoaSyncRef>
        {
            new() {Ni = "0004", ExternalId = "30004"},
        };

        // Act
        await using var actContext = NewContext();
        var result = await new ZhrFetcherByNi(actContext).ExecuteAsync<ZhrSAptidao>(inputs, _ct);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnRowsForMultipleNis_WhenAllExistInTable()
    {
        // Arrange
        string ni1 = "0001", externalId1 = "30001";
        string ni2 = "0002", externalId2 = "30002";


        await SeedAsync(
            AptidaoOutput(ni1, externalId1),
           [AptidaoChild(ni1, "Aptidao1"), AptidaoChild(ni1, "Aptidao2")]);

        await SeedAsync(
            AptidaoOutput(ni2, externalId2),
            [AptidaoChild(ni2, "Aptidao1"), AptidaoChild(ni2, "Aptidao2")]);

        var inputs = new List<PessoaSyncRef>
        {
            new() {Ni = ni1, ExternalId = externalId1},
            new() {Ni = ni2, ExternalId = externalId2},
        };


        // Act
        await using var actContext = NewContext();
        var result = await new ZhrFetcherByNi(actContext).ExecuteAsync<ZhrSAptidao>(inputs, _ct);

        // Assert
        result.Should().HaveCount(4);
        result.Where(i => i.Ni == ni1).Should().HaveCount(2);
        result.Where(i => i.Ni == ni2).Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldReturnOnlyMatchingNis_WhenPartialNisExistInTable()
    {
        // Arrange
        string ni1 = "0001", externalId1 = "30001";
        string ni2 = "0002", externalId2 = "30002";
        string ni3 = "0003", externalId3 = "30003";

        await SeedAsync(AptidaoOutput(ni1, externalId1), [AptidaoChild(ni1, "Aptidao1")]);
        await SeedAsync(AptidaoOutput(ni2, externalId2), [AptidaoChild(ni2, "Aptidao1")]);

        var inputs = new List<PessoaSyncRef>
        {
            new() {Ni = ni1, ExternalId = externalId1},
            new() {Ni = ni2, ExternalId = externalId2},
            new() {Ni = ni3, ExternalId = externalId3},
        };

        // Act
        await using var actContext = NewContext();
        var result = await new ZhrFetcherByNi(actContext).ExecuteAsync<ZhrSAptidao>(inputs, _ct);

        // Assert
        result.Should().HaveCount(2);
        result.Where(i => i.Ni == ni1).Should().HaveCount(1);
        result.Where(i => i.Ni == ni2).Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        string ni1 = "0001", externalId1 = "30001";
        string ni2 = "0002", externalId2 = "30002";
        string ni3 = "0003", externalId3 = "30003";

        await SeedAsync(AptidaoOutput(ni1, externalId1), [AptidaoChild(ni1, "Aptidao1")]);
        await SeedAsync(AptidaoOutput(ni2, externalId2), [AptidaoChild(ni2, "Aptidao1")]);
        await SeedAsync(AptidaoOutput(ni3, externalId3), [AptidaoChild(ni3, "Aptidao1")]);


        // Act
        await using var actContext = NewContext();
        var result = await new ZhrFetcherByNi(actContext).ExecuteAsync<ZhrSAptidao>([], _ct);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnLargeList_WhenInputIsLarge()
    {
        // Arrange
        var inputs = new List<PessoaSyncRef>();
        for (int i = 0; i < 1_000; i++)
        {
            string ni = $"000{i}", externalId = $"3000{i}";
            inputs.Add(new PessoaSyncRef { Ni = ni, ExternalId = externalId });
            await SeedAsync(AptidaoOutput(ni, externalId), [AptidaoChild(ni, "Aptidao1")]);
        }

        // Act
        await using var actContext = NewContext();
        var result = await new ZhrFetcherByNi(actContext).ExecuteAsync<ZhrSAptidao>(inputs, _ct);

        // Assert
        result.Should().HaveCount(1000);
    }

}

namespace SigdnRhStaggingApi.Tests;

using SigdnRhStaggingApi.Models;
using Snapshooter.Xunit;

public class IntegrationTests
{

    [Fact]
    public async Task SchemaChangeTest()
    {
        var schema = await TestServices.Executor.GetSchemaAsync(default);
        schema.ToString().MatchSnapshot();
    }

    [Fact]
    public async Task FetchEmployeesWithEmptyDB()
    {
        var (scope, dbContext) = await TestServices.CreateScopeAndDbContextAsync();
        using (scope)
        await using (dbContext)
        {
            var result = await TestServices.ExecuteRequestAsync(scope.ServiceProvider, b => b.SetDocument("{ employees { ni numsap id } }"));
            result.MatchSnapshot();
        }
    }

    [Fact]
    public async Task FetchEmployeesWithSeededData()
    {
        var (scope, dbContext) = await TestServices.CreateScopeAndDbContextAsync();
        using (scope)
        await using (dbContext)
        {

            var employees = new List<Employee> {
            new() { Ni = "11111", Numsap = "30001111", BiometricDetails = new() },
            new() { Ni = "11112", Numsap = "30001112", BiometricDetails = new() }
            };
            dbContext.Employees.AddRange(employees);
            await dbContext.SaveChangesAsync();

            var result = await TestServices.ExecuteRequestAsync(scope.ServiceProvider, b => b.SetDocument("{ employees { ni numsap id } }"));
            result.MatchSnapshot();
        }
    }
}
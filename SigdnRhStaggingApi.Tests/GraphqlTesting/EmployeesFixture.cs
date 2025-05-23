using Microsoft.Extensions.DependencyInjection;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Models;

namespace SigdnRhStaggingApi.Tests.GraphqlTesting;

public class EmployeesFixture : IAsyncLifetime
{
    public IServiceScope Scope { get; }
    public RhStaggingDbContext DbContext { get; }

    public GraphqlTestServices TestServices { get; }

    public EmployeesFixture()
    {
        TestServices = new GraphqlTestServices();
        (Scope, DbContext) = TestServices.CreateScopeAndDbContextAsync().GetAwaiter().GetResult();
    }

    public async Task InitializeAsync()
    {

        var employees = new List<Employee> {
            new() { Ni = "11111", Numsap = "30001111", BiometricDetails = new() },
            new() { Ni = "11112", Numsap = "30001112", BiometricDetails = new() },
            new() { Ni = "11110", Numsap = "30001110", BiometricDetails = new() {
                EyesColor = "brown",
                BloodType ="A+",
                HeightCm = "180"
                }
            },

        };
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        Scope.Dispose();
    }
}

[CollectionDefinition("Employees Collection")]
public class EmployeesCollection : ICollectionFixture<EmployeesFixture> { }
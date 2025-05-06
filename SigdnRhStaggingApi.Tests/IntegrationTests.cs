namespace SigdnRhStaggingApi.Tests;

using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Models;
using Snapshooter.Xunit;

public class IntegrationTests
{

    [Fact]
    public async Task SchemaChangeTest()
    {
        var testService = new TestServices("TestDbSchema");

        var schema = await testService.Executor.GetSchemaAsync(default);
        schema.ToString().MatchSnapshot();
    }

    [Fact]
    public async Task FetchEmployeesWithEmptyDB()
    {
        var testService = new TestServices("TestDbEmpty");

        var (scope, dbContext) = await testService.CreateScopeAndDbContextAsync();
        using (scope)
        await using (dbContext)
        {
            var result = await testService.ExecuteRequestAsync(b => b.SetDocument("{ employees { ni numsap id } }"));
            result.MatchSnapshot();
        }
    }

}


[Collection("Employees Collection")]
public class EmployeeQueriesTests(EmployeesFixture fixture)
{
    private readonly EmployeesFixture _fixture = fixture;

    [Fact]
    public async Task FetchAllEmployeesWithNiOnly()
    {
        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument("{ employees { ni } }")
        );

        result.MatchSnapshot();
    }
    [Fact]
    public async Task FetchAllEmployeesWithAllFields()
    {
        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument("{ employees { id ni numsap biometricDetails { eyesColor heightCm bloodType } } }")
        );

        result.MatchSnapshot();
    }

    [Fact]
    public async Task FetchAllEmployeesSorted()
    {
        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument("{ employees(order: {ni: ASC}) { ni } }")
        );
        result.MatchSnapshot();
    }

    [Fact]
    public async Task FetchFilteredEmployee()
    {
        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument("{ employees(where: { ni: { eq: \"11111\" } }) { ni } }")
        );
        result.MatchSnapshot();
    }

    [Fact]
    public async Task FetchFilteredEmployees()
    {
        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument("{ employees(where: { ni: { in: [\"11111\", \"11112\"] } }) { ni } }")
        );
        result.MatchSnapshot();
    }
}

public class EmployeeMutationsTests
{
    [Fact]
    public async Task AddEmployeeMutation()
    {

        // Arrange
        var mutation = @"
        mutation {
            addEmployee(input: {
                employeeDto: {
                    ni: ""22600"",
                    numsap: ""30002697""
                }
            }) {
                employeeDto {
                    id
                    ni
                    numsap
                    biometricDetailsDto {
                        eyesColor
                        heightCm
                        bloodType
                    }
                }
            }
        }";

        var testService = new TestServices("MutationDbTest");

        var (scope, dbContext) = await testService.CreateScopeAndDbContextAsync();
        using (scope)
        await using (dbContext)
        {
            var result = await testService.ExecuteRequestAsync(b => b.SetDocument(mutation));
            var inTest = await dbContext.Employees.Where(employee => employee.Ni == "22600").FirstAsync();
            Assert.Equal("22600", inTest.Ni);
            Assert.Equal("30002697", inTest.Numsap);
            result.MatchSnapshot();
        }
    }

    [Fact]
    public async Task RemoveEmployeeMutation()
    {
        var testService = new TestServices("RemoveDbTest");

        var (scope, dbContext) = await testService.CreateScopeAndDbContextAsync();

        // Arrange
        var employee = new Employee
        {
            Ni = "11111",
            Numsap = "30001111",
            BiometricDetails = new BiometricDetails
            {
                EyesColor = "brown",
                BloodType = "A+",
                HeightCm = "180"
            }
        };

        var employeeAdded = dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var mutation = $@"
        mutation {{
            removeEmployee(input: {{ id: {employeeAdded.Entity.Id} }}) {{
                employeeDto {{ 
                    id
                    ni
                    numsap
                }}
            }}
        }}";

        using (scope)
        await using (dbContext)
        {
            var result = await testService.ExecuteRequestAsync(b => b.SetDocument(mutation));
            result.MatchSnapshot();
            var inTest = await dbContext.Employees.ToListAsync();
            Assert.Empty(inTest);

        }

    }
}
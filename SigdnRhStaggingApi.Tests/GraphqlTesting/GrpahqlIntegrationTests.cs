using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Models;
using Snapshooter.Xunit;

namespace SigdnRhStaggingApi.Tests.GraphqlTesting;

public class GraphqlIntegrationTests
{

    [Fact]
    public async Task SchemaChangeTest()
    {
        var testService = new GraphqlTestServices();

        var schema = await testService.Executor.GetSchemaAsync(default);
        schema.ToString().MatchSnapshot();
    }

    [Fact]
    public async Task FetchEmployeesWithEmptyDB()
    {
        var testService = new GraphqlTestServices();

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

[Collection("Employees Collection")]
public class EmployeeByNiQueriesTests(EmployeesFixture fixture)
{
    private readonly EmployeesFixture _fixture = fixture;

    private static ClaimsPrincipal GetPrincipal(string principalUsername)
    {
        return new(
           new ClaimsIdentity(
               [
                   new Claim(ClaimTypes.Name, principalUsername),
                   new Claim("employeeId", principalUsername[1..])
               ],
               authenticationType: "TestAuth"
               )
           );
    }

    private static string GetEmployeeByNiQuery(string ni)
    {
        return $@"
                {{
                    employeeByNi(ni: ""{ni}"") {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                          bloodType
                          eyesColor
                          heightCm
                        }}
                    }}
                }}
            ";
    }


    [Fact]
    public async Task GetEmployeeByNi_ByAuthorizedUser_ReturnsEmployee()
    {
        var ni = "11111";
        var principalUsername = "m11111";

        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(GetEmployeeByNiQuery(ni)).AddGlobalState(nameof(ClaimsPrincipal), GetPrincipal(principalUsername)),
           principalUsername
        );

        result.MatchSnapshot();
    }

    [Fact]
    public async Task GetEmployeeByNi_ByNotAuthorizedUser_ReturnsForbidden()
    {
        var ni = "11112";
        var principalUsername = "m11111";

        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(GetEmployeeByNiQuery(ni)).AddGlobalState(nameof(ClaimsPrincipal), GetPrincipal(principalUsername)),
            principalUsername
        );

        result.MatchSnapshot();
    }

    [Fact]
    public async Task GetEmployeeByNi_WithNoHttpContext_ReturnsNotAuthenticated()
    {
        var ni = "11111";
        var principalUsername = "m11111";
        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(GetEmployeeByNiQuery(ni)).AddGlobalState(nameof(ClaimsPrincipal), GetPrincipal(principalUsername)));

        result.MatchSnapshot();
    }


    [Fact]
    public async Task GetEmployeeByNi_ByNotAuthenticatedUser_ReturnsNotAuthenticated()
    {
        var ni = "11111";
        ClaimsPrincipal principal = new();

        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(GetEmployeeByNiQuery(ni)).AddGlobalState(nameof(ClaimsPrincipal), principal));
        result.MatchSnapshot();
    }

    [Fact]
    public async Task GetEmployeeByNi__NonExistingEmployee__ReturnsNotFoundEmployee()
    {
        var ni = "22600";
        var principalUsername = "m22600";

        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(GetEmployeeByNiQuery(ni)).AddGlobalState(nameof(ClaimsPrincipal), GetPrincipal(principalUsername)),
            principalUsername
        );

        result.MatchSnapshot();
    }

    [Fact]
    public async Task GetEmployeeByNiWithMultipleFields__OnlyOneAuthorized_ReturnsAuthorizedEmployeeOnlyAndForbiddenForNonAuthorizedEmployees()
    {
        var ni = "11111";
        var principalUsername = "m11111";

        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(
            $@"
                {{
                    a: employeeByNi(ni: ""{ni}"") {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                            bloodType
                            eyesColor
                            heightCm
                        }}
                    }}
                    b: employeeByNi(ni: ""11112"") {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                            bloodType
                            eyesColor
                            heightCm
                        }}
                    }}
                    c: employeeByNi(ni: ""11110"")    {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                            bloodType
                            eyesColor
                            heightCm
                        }}
                    }}
                }}
            ")
            .AddGlobalState(nameof(ClaimsPrincipal), GetPrincipal(principalUsername)),
            principalUsername
        );

        result.MatchSnapshot();
    }

    [Fact]
    public async Task GetEmployeeByNiWithMultipleFields__OnlyOneAuthorizedButNotFound_ReturnsNotFoundEmployeeAndForbiddenForNonAuthorized()
    {
        var ni = "22600";
        var principalUsername = "m22600";

        var result = await _fixture.TestServices.ExecuteRequestAsync(
            b => b.SetDocument(
            $@"
                {{
                    a: employeeByNi(ni: ""{ni}"") {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                            bloodType
                            eyesColor
                            heightCm
                        }}
                    }}
                    b: employeeByNi(ni: ""11112"") {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                            bloodType
                            eyesColor
                            heightCm
                        }}
                    }}
                    c: employeeByNi(ni: ""11110"")    {{
                        numsap
                        ni
                        id
                        biometricDetails {{
                            bloodType
                            eyesColor
                            heightCm
                        }}
                    }}
                }}
            ")
            .AddGlobalState(nameof(ClaimsPrincipal), GetPrincipal(principalUsername)),
            principalUsername
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
            mutation AddEmployee {
                addEmployee(
                    input: {
                        employee: {
                                biometricDetails: {
                                    bloodType: ""O+"",
                                    eyesColor: ""brown"",
                                    heightCm: ""176""
                                }
                                ni: ""22600"",
                                numsap: ""30002697""
                            }
                        }
                ) {
                    employee {
                      id
                      ni
                      numsap
                      biometricDetails {
                        bloodType
                        eyesColor
                        heightCm
                      }
                    }
                  }
                }
        ";

        var testService = new GraphqlTestServices();

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
    public async Task AddEmployeeMutationDuplicatedExpectsEmployeeDucplicatedException()
    {

        // Arrange
        var testService = new GraphqlTestServices();

        var (scope, dbContext) = await testService.CreateScopeAndDbContextAsync();
        var employee = new Employee
        {
            Ni = "11111",
            Numsap = "30001111",
            BiometricDetails = new BiometricDetails
            {
                EyesColor = "brown",
                BloodType = "O+",
                HeightCm = "176"
            }
        };
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var mutation = @"
            mutation AddEmployee {
                addEmployee(
                    input: {
                        employee: {
                                biometricDetails: {
                                    bloodType: ""O+"",
                                    eyesColor: ""brown"",
                                    heightCm: ""176""
                                }
                                ni: ""11111"",
                                numsap: ""30001111""
                            }
                        }
                ) {
                    employee {
                      id
                      ni
                      numsap
                    }
                  }
                }
        ";

        using (scope)
        await using (dbContext)
        {
            var result = await testService.ExecuteRequestAsync(b => b.SetDocument(mutation));
            result.MatchSnapshot();
        }
    }

    [Fact]
    public async Task RemoveEmployeeMutation()
    {
        var testService = new GraphqlTestServices();

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
                boolean
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

    [Fact]
    public async Task RemoveNoExistingEmployeeMutationExpectsEmployeeNotFoundException()
    {
        var testService = new GraphqlTestServices();

        var (scope, dbContext) = await testService.CreateScopeAndDbContextAsync();

        var mutation = @"
        mutation {
            removeEmployee(input: { id: 1 }) {
                boolean
            }
        }";

        using (scope)
        await using (dbContext)
        {
            var result = await testService.ExecuteRequestAsync(b => b.SetDocument(mutation));
            result.MatchSnapshot();
        }

    }
}
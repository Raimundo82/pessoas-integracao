using HotChocolate.Authorization;
using SigdnRhStaggingApi.Models;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Employees;

[QueryType]
public static class EmployeesQueries
{
    [AllowAnonymous]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [UseReadApiKeyMiddleware]
    public static IQueryable<Employee> GetEmployees(IEmployeeService employeeService) => employeeService.GetEmployees();

    public static Task<Employee?> GetEmployeeByNi(string ni, IEmployeeService employeeService, CancellationToken cancellationToken) =>
        employeeService.GetEmployeeByNi(ni, cancellationToken);

}
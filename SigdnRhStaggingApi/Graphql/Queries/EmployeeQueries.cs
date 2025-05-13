using HotChocolate.Authorization;
using SigdnRhStaggingApi.Models;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Queries;

[QueryType]
public static class EmployeesQueries
{
    [AllowAnonymous]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<Employee> GetEmployees(IEmployeeService employeeService) =>
                employeeService.GetEmployees();
}
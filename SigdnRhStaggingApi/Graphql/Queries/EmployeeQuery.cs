using HotChocolate.Authorization;
using SigdnRhStaggingApi.Models;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Queries;
public class EmployeeQuery
{
    [AllowAnonymous]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Employee> GetEmployees(IEmployeeService employeeService) =>
                employeeService.GetEmployees();
}
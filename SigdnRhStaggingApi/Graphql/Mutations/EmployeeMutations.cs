using SigdnRhStaggingApi.Graphql.Exceptions;
using SigdnRhStaggingApi.Graphql.Inputs;
using SigdnRhStaggingApi.Models;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Mutations;

[MutationType]
public static class EmployeeMutations
{
    [UseWriteApiKeyMiddleware]
    [Error(typeof(EmployeeDuplicatedException))]
    public static async Task<Employee> AddEmployee(EmployeeInput employee, IEmployeeService employeeService)
    {
        return await employeeService.AddEmployee(employee);
    }

    [UseWriteApiKeyMiddleware]
    [Error(typeof(EmployeeNotFoundException))]
    public static async Task<bool?> RemoveEmployee(int id, IEmployeeService employeeService)
    {
        return await employeeService.DeleteEmployee(id);
    }
}
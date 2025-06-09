using SigdnRhStaggingApi.Models;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Employees;

[MutationType]
public static class EmployeeMutations
{
    [UseWriteApiKeyMiddleware]
    public static async Task<Employee> AddEmployee(EmployeeInput employee, IEmployeeService employeeService)
    {
        return await employeeService.AddEmployee(employee);
    }

    [UseWriteApiKeyMiddleware]
    public static async Task<bool?> RemoveEmployee(int id, IEmployeeService employeeService)
    {
        return await employeeService.DeleteEmployee(id);
    }
}
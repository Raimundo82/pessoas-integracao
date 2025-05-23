using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Mutations;

[MutationType]
public static class EmployeeMutations
{
    [UseWriteApiKeyMiddleware]
    public static async Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto, IEmployeeService employeeService)
    {
        return await employeeService.AddEmployee(employeeDto);
    }

    [UseWriteApiKeyMiddleware]
    public static async Task<EmployeeDto?> RemoveEmployee(int id, IEmployeeService employeeService)
    {
        return await employeeService.DeleteEmployee(id);
    }
}
using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Mutations;

[MutationType]
public static class EmployeeMutations
{
    public static async Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto, IEmployeeService employeeService)
    {
        return await employeeService.AddEmployee(employeeDto);
    }

    public static async Task<EmployeeDto?> RemoveEmployee(int id, IEmployeeService employeeService)
    {
        return await employeeService.DeleteEmployee(id);
    }
}
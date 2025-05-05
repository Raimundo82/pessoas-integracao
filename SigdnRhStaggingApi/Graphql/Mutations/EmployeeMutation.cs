using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Mutations;

public class EmployeeMutation
{
    public async Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto, IEmployeeService employeeService)
    {
        return await employeeService.AddEmployee(employeeDto);
    }

    public async Task<EmployeeDto?> RemoveEmployee(int id, IEmployeeService employeeService)
    {
        return await employeeService.DeleteEmployee(id);
    }
}

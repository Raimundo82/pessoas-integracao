
using SigdnRhStaggingApi.DTOs;

namespace SigdnRhStaggingApi.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetEmployees();
    Task<EmployeeDto?> GetEmployeeById(int id);
    Task<EmployeeDto?> GetEmployeeByNi(string ni);
    Task<EmployeeDto?> GetEmployeeByNumsap(string numsap);

    Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto);
    Task<EmployeeDto?> EditEmployee(int id, EmployeeDto employeeDto);
    Task<bool> DeleteEmployee(int id);

}


using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Models;

namespace SigdnRhStaggingApi.Services;

public interface IEmployeeService
{
    IQueryable<Employee> GetEmployees();
    Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto);

    Task<EmployeeDto?> DeleteEmployee(int id);


}
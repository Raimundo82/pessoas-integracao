using SigdnRhStaggingApi.Graphql.Employees;
using SigdnRhStaggingApi.Models;

namespace SigdnRhStaggingApi.Services;

public interface IEmployeeService
{
    IQueryable<Employee> GetEmployees();
    Task<Employee> AddEmployee(EmployeeInput employee);
    Task<bool> DeleteEmployee(int id);
}
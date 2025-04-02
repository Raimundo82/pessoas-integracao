using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Models;

namespace SigdnRhStaggingApi.Services;

public class EmployeeService(RhStaggingDbContext dbContext) : IEmployeeService
{
    private readonly RhStaggingDbContext _context = dbContext;

    private static EmployeeDto GetEmployeeDto(Employee employee)
    {

        return new EmployeeDto
        {
            Id = employee.Id,
            Numsap = employee.Numsap,
            Ni = employee.Ni
        };
    }

    public async Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto)
    {
        Employee employee = new()
        {
            Numsap = employeeDto.Numsap,
            Ni = employeeDto.Ni,
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return GetEmployeeDto(employee);
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EmployeeDto?> EditEmployee(int id, EmployeeDto employeeDto)
    {
        Employee? employee = await _context.Employees.FindAsync(id);
        if (employee == null) return null;

        employee.Ni = employeeDto.Ni;
        employee.Numsap = employeeDto.Numsap;
        await _context.SaveChangesAsync();
        return GetEmployeeDto(employee);
    }

    public async Task<EmployeeDto?> GetEmployeeById(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        return employee is not null ? GetEmployeeDto(employee) : null;
    }

    public async Task<EmployeeDto?> GetEmployeeByNi(string ni)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(emp => emp.Ni == ni);
        return employee is not null ? GetEmployeeDto(employee) : null;
    }

    public async Task<EmployeeDto?> GetEmployeeByNumsap(string numsap)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(emp => emp.Numsap == numsap);
        return employee is not null ? GetEmployeeDto(employee) : null;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployees()
    {
        return await _context
            .Employees
            .Select(employee => GetEmployeeDto(employee))
            .ToListAsync();
    }
}

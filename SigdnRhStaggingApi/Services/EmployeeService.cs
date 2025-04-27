using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Models;

namespace SigdnRhStaggingApi.Services;

// CA1816: Call GC.SuppressFinalize correctly
// https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816#how-to-fix-violations
// If the type is not meant to be overridden, mark it as sealed.
public sealed class EmployeeService(
        IDbContextFactory<RhStaggingDbContext> dbContextFactory,
        ILogger<EmployeeService> logger)
        : IEmployeeService, IAsyncDisposable
{
    private readonly RhStaggingDbContext dbContext = dbContextFactory.CreateDbContext();
    private readonly ILogger<EmployeeService> _logger = logger;
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
        _logger.LogInformation("Adding employee with Numsap: {Numsap}", employeeDto.Numsap);

        Employee employee = new()
        {
            Numsap = employeeDto.Numsap,
            Ni = employeeDto.Ni,
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();
        return GetEmployeeDto(employee);
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        var employee = await dbContext.Employees.FindAsync(id);
        if (employee == null) return false;
        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<EmployeeDto?> EditEmployee(int id, EmployeeDto employeeDto)
    {
        Employee? employee = await dbContext.Employees.FindAsync(id);
        if (employee == null) return null;

        employee.Ni = employeeDto.Ni;
        employee.Numsap = employeeDto.Numsap;
        await dbContext.SaveChangesAsync();
        return GetEmployeeDto(employee);
    }

    public async Task<EmployeeDto?> GetEmployeeById(int id)
    {
        _logger.LogInformation("Fetching employee with Id: {Id}", id);

        var employee = await dbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Employee with Id: {Id} not found", id);
        }
        return employee is not null ? GetEmployeeDto(employee) : null;
    }

    public async Task<EmployeeDto?> GetEmployeeByNi(string ni)
    {
        _logger.LogInformation("Fetching employee with Ni: {Ni}", ni);

        var employee = await dbContext.Employees.FirstOrDefaultAsync(emp => emp.Ni == ni);
        return employee is not null ? GetEmployeeDto(employee) : null;
    }

    public async Task<EmployeeDto?> GetEmployeeByNumsap(string numsap)
    {
        _logger.LogInformation("Fetching employee with Numsap: {Numsap}", numsap);

        var employee = await dbContext.Employees.FirstOrDefaultAsync(emp => emp.Numsap == numsap);
        return employee is not null ? GetEmployeeDto(employee) : null;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployees()
    {
        _logger.LogInformation("Fetching all employees");

        return await dbContext
            .Employees
            .Select(employee => GetEmployeeDto(employee))
            .ToListAsync();
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }
}

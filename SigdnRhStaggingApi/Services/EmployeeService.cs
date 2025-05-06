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

    private static BiometricDetailsDto GetBiometricDetailsDto(BiometricDetails biometricDetails)
    {
        return new BiometricDetailsDto
        {
            EyesColor = biometricDetails.EyesColor,
            BloodType = biometricDetails.BloodType,
            HeightCm = biometricDetails.HeightCm
        };
    }
    private static EmployeeDto GetEmployeeDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Numsap = employee.Numsap,
            Ni = employee.Ni,
            BiometricDetailsDto = GetBiometricDetailsDto(employee.BiometricDetails)
        };
    }
    public IQueryable<Employee> GetEmployees() => dbContext.Employees;

    public async Task<EmployeeDto> AddEmployee(EmployeeDto employeeDto)
    {
        _logger.LogInformation("Adding employee with Numsap: {Numsap}", employeeDto.Numsap);

        Employee employee = new()
        {
            Numsap = employeeDto.Numsap,
            Ni = employeeDto.Ni,
            BiometricDetails = new()
            {
                EyesColor = employeeDto.BiometricDetailsDto?.EyesColor,
                HeightCm = employeeDto.BiometricDetailsDto?.HeightCm,
                BloodType = employeeDto.BiometricDetailsDto?.BloodType
            }
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();
        return GetEmployeeDto(employee);
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }

    public async Task<EmployeeDto?> DeleteEmployee(int id)
    {
        var employee = await dbContext.Employees.FindAsync(id);
        if (employee == null) return null;
        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync();
        return GetEmployeeDto(employee);
    }
}
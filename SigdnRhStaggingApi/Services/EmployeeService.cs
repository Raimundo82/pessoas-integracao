using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Graphql.Employees;
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

    public IQueryable<Employee> GetEmployees() => dbContext.Employees;

    public async Task<Employee> AddEmployee(EmployeeInput employee)
    {
        _logger.LogInformation("Adding employee with Numsap {Numsap} and Ni {Ni}", employee.Numsap, employee.Ni);

        var employeeExists = await dbContext.Employees.AnyAsync(e => e.Ni == employee.Ni);
        if (employeeExists) throw new EmployeeDuplicatedException(employee.Ni);

        Employee newEmployee = new()
        {
            Numsap = employee.Numsap,
            Ni = employee.Ni,
            BiometricDetails = new()
            {
                EyesColor = employee.BiometricDetails?.EyesColor,
                HeightCm = employee.BiometricDetails?.HeightCm,
                BloodType = employee.BiometricDetails?.BloodType
            }
        };



        var employeeAdded = dbContext.Employees.Add(newEmployee).Entity;
        await dbContext.SaveChangesAsync();
        return employeeAdded;
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        var employee = await dbContext.Employees.FindAsync(id) ?? throw new EmployeeByIdNotFoundException(id);
        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Employee?> GetEmployeeByNi(string ni, CancellationToken cancellationToken)
    {
        var employees = await dbContext
                .Employees
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        return employees.FirstOrDefault(e => e.Ni == ni) ?? throw new EmployeeByNiNotFoundException(ni);
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }
}
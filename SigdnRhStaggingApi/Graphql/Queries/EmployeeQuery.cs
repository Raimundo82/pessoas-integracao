using HotChocolate.Authorization;
using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Queries
{
    public class EmployeeQuery()
    {
        [AllowAnonymous]
        public async Task<IEnumerable<EmployeeDto>> GetEmployees(IEmployeeService employeeService)
        {
            return await employeeService.GetEmployees();
        }
    }
}
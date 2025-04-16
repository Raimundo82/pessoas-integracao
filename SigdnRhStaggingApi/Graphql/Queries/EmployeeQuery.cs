using HotChocolate.Authorization;
using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql.Queries
{
    public class EmployeeQuery(IEmployeeService employeeService)
    {
        private readonly IEmployeeService _employeeService = employeeService;
        [AllowAnonymous]
        public async Task<IEnumerable<EmployeeDto>> GetEmployees()
        {
            return await _employeeService.GetEmployees();
        }
    }
}
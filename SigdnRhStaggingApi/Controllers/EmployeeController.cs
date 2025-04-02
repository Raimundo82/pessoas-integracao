using Microsoft.AspNetCore.Mvc;
using SigdnRhStaggingApi.DTOs;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    private readonly IEmployeeService _employeeService = employeeService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        return Ok(await _employeeService.GetEmployees());
    }
}

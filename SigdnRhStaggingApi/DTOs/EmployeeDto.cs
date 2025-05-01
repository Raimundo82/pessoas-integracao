namespace SigdnRhStaggingApi.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public required string Numsap { get; set; }
    public required string Ni { get; set; }
    public BiometricDetailsDto? BiometricDetailsDto { get; set; }
}

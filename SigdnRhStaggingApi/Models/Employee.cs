namespace SigdnRhStaggingApi.Models;
public class Employee
{
    public int Id { get; set; }
    public required string Numsap { get; set; }
    public required string Ni { get; set; }
    public required BiometricDetails BiometricDetails { get; set; }
}
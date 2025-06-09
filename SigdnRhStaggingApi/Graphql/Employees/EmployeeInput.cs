namespace SigdnRhStaggingApi.Graphql.Employees;

public record class EmployeeInput(
    int? Id,
    string Numsap,
    string Ni,
    BiometricDetailsInput BiometricDetails
);
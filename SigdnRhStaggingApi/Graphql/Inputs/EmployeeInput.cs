namespace SigdnRhStaggingApi.Graphql.Inputs;

public record class EmployeeInput(
    int? Id,
    string Numsap,
    string Ni,
    BiometricDetailsInput BiometricDetails
);
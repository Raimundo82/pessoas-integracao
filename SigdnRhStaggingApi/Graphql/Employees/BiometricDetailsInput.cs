namespace SigdnRhStaggingApi.Graphql.Employees;

public record class BiometricDetailsInput(
    string BloodType,
    string EyesColor,
    string HeightCm
);
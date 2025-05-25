namespace SigdnRhStaggingApi.Graphql.Inputs;

public record class BiometricDetailsInput(
    string BloodType,
    string EyesColor,
    string HeightCm
);
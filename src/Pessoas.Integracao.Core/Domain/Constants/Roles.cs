namespace Pessoas.Integracao.Core.Domain.Constants;

public static class Roles
{
    public const string Admin = "admin";
    public const string Viewer = "viewer";

    public static string? FromExternalProvider(string externalRole)
    {
        if (externalRole.Equals(Admin, StringComparison.OrdinalIgnoreCase))
        {
            return Admin;

        }
        else if (externalRole.Equals(Viewer, StringComparison.OrdinalIgnoreCase))
        {
            return Viewer;
        }
        return null;
    }
}
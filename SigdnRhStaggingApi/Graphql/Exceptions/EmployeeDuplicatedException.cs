namespace SigdnRhStaggingApi.Graphql.Exceptions;

public class EmployeeDuplicatedException(string ni) : Exception($"Employee with NI {ni} already exists.")
{
    public string Ni { get; } = ni;
}
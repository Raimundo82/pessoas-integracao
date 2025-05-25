namespace SigdnRhStaggingApi.Graphql.Exceptions;

public class EmployeeNotFoundException(int id) : Exception($"The employee with Id {id} was not found.")
{
    public int Id { get; } = id;

}
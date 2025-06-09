namespace SigdnRhStaggingApi.Graphql.Employees;

public abstract class EmployeeException(string message) : GraphQLException(CreateError(message))
{
    public static IError CreateError(string message) =>
        ErrorBuilder
            .New()
            .SetMessage(message)
            .Build();
}

public sealed class EmployeeDuplicatedException(string ni) : EmployeeException($"Employee with NI {ni} already exists.")
{
};

public sealed class EmployeeByNiNotFoundException(string ni) : EmployeeException($"Employee with NI {ni} not found.")
{
};

public sealed class EmployeeByIdNotFoundException(int id) : EmployeeException($"Employee with ID {id} not found.")
{
};
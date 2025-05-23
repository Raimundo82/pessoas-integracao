using System.Net;
using System.Net.Http.Json;

namespace SigdnRhStaggingApi.Tests.HttpTesting;

public class HttpIntegrationTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetDefaultEndpointNotFoundtatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();
        var url = "/";

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task GetGrpahqlSchemaEndpointSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();
        var url = "/graphql/schema.graphql"; // Define the URL directly here

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task QueryEmployeesWithoutApiKeyHeaderUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = "{ employees { ni numsap id } }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QueryEmployeesWithApiKeyHeaderUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "wrong-key");
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = "{ employees { ni numsap id } }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QueryEmployeesWithCorrectReadApiKey()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "read-key");
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = "{ employees { ni numsap id } }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task QueryEmployeesWithCorrectWriteApiKey()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "write-key");
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = "{ employees { ni numsap id } }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MutateEmployeeWithoutApiKey()
    {
        // Arrange
        var client = _factory.CreateClient();
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = @"
                        mutation {
                            addEmployee(input: {
                                employeeDto: {
                                    ni: ""22600"",
                                    numsap: ""30002697""
                                }
                            }) {
                                employeeDto {
                                    id
                                    ni
                                    numsap
                                    biometricDetailsDto {
                                        eyesColor
                                        heightCm
                                        bloodType
                                    }
                                }
                            }
                        }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MutateEmployeeWithWrongApiKey()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "wrong-key");
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = @"
                        mutation {
                            addEmployee(input: {
                                employeeDto: {
                                    ni: ""22600"",
                                    numsap: ""30002697""
                                }
                            }) {
                                employeeDto {
                                    id
                                    ni
                                    numsap
                                    biometricDetailsDto {
                                        eyesColor
                                        heightCm
                                        bloodType
                                    }
                                }
                            }
                        }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MutateEmployeeWithCorrectReadApiKey()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "read-key");
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = @"
                        mutation {
                            addEmployee(input: {
                                employeeDto: {
                                    ni: ""22600"",
                                    numsap: ""30002697""
                                }
                            }) {
                                employeeDto {
                                    id
                                    ni
                                    numsap
                                    biometricDetailsDto {
                                        eyesColor
                                        heightCm
                                        bloodType
                                    }
                                }
                            }
                        }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MutateEmployeeWithCorrectWriteApiKey()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", "write-key");
        var url = "/graphql";

        // Act
        var response = await client.PostAsJsonAsync(url, new
        {
            query = @"
                        mutation {
                            addEmployee(input: {
                                employeeDto: {
                                    ni: ""22600"",
                                    numsap: ""30002697""
                                }
                            }) {
                                employeeDto {
                                    id
                                    ni
                                    numsap
                                    biometricDetailsDto {
                                        eyesColor
                                        heightCm
                                        bloodType
                                    }
                                }
                            }
                        }"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
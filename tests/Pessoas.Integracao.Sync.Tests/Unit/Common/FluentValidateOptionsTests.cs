using FluentAssertions;

using FluentValidation;
using FluentValidation.Results;

using Moq;

using Pessoas.Integracao.Sync.Infrastructure.Common;

namespace Pessoas.Integracao.Sync.Tests.Unit.Common;

public class FluentValidateOptionsTests
{
    [Fact]
    public void ShouldFail_WhenOptionsAreNull()
    {
        // Arrange
        var validator = new Mock<IValidator<TestOptions>>(MockBehavior.Strict);
        var fluentValidateOptions = new FluentValidateOptions<TestOptions>(validator.Object);

        // Act
        var result = fluentValidateOptions.Validate(name: null, options: null!);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("TestOptions options not found.", result.FailureMessage);
    }

    [Fact]
    public void ShouldSuccess_WhenOptionsAreValid()
    {
        // Arrange
        var validator = new Mock<IValidator<TestOptions>>(MockBehavior.Strict);
        validator
            .Setup(x => x.Validate(It.IsAny<TestOptions>()))
            .Returns(new ValidationResult());

        var fluentValidateOptions = new FluentValidateOptions<TestOptions>(validator.Object);
        var options = new TestOptions();

        // Act
        var result = fluentValidateOptions.Validate(name: null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.FailureMessage.Should().BeNull();
        validator.Verify(x => x.Validate(options), Times.Once);
    }

    [Fact]
    public void ShouldFailWithErrorMessage_WhenInvalidOptionsWithSingleError()
    {
        // Arrange
        var validator = new Mock<IValidator<TestOptions>>(MockBehavior.Strict);
        validator
            .Setup(x => x.Validate(It.IsAny<TestOptions>()))
            .Returns(new ValidationResult(
                new List<ValidationFailure> { new("property", "Error Message") }
            ));

        var fluentValidateOptions = new FluentValidateOptions<TestOptions>(validator.Object);
        var options = new TestOptions();

        // Act
        var result = fluentValidateOptions.Validate(name: null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().NotBeNull();
        result.Failures.Should().HaveCount(1);
        result.FailureMessage.Should().Be("Error Message");
        validator.Verify(x => x.Validate(options), Times.Once);
    }

    [Fact]
    public void ShouldFailWithMultipleErrorMessage_WhenInvalidOptionsHaveMultipleErrors()
    {
        // Arrange
        var validator = new Mock<IValidator<TestOptions>>(MockBehavior.Strict);
        validator
            .Setup(x => x.Validate(It.IsAny<TestOptions>()))
            .Returns(new ValidationResult(
                new List<ValidationFailure>
                {
                    new("propertyOne", "Error Message One"),
                    new("propertyTwo", "Error Message Two")
                }
            ));

        var fluentValidateOptions = new FluentValidateOptions<TestOptions>(validator.Object);
        var options = new TestOptions();

        // Act
        var result = fluentValidateOptions.Validate(name: null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().NotBeNull();
        result.Failures.Should().HaveCount(1);
        result.FailureMessage.Should().Be("Error Message One Error Message Two");
        validator.Verify(x => x.Validate(options), Times.Once);
    }

    [Fact]
    public void ShouldFailWithNoErrorMessage_WhenInvalidOptionsAndEmptyError()
    {
        // Arrange
        var validator = new Mock<IValidator<TestOptions>>(MockBehavior.Strict);
        validator
            .Setup(x => x.Validate(It.IsAny<TestOptions>()))
            .Returns(new ValidationResult(
                new List<ValidationFailure> { new("property", "") }
            ));

        var fluentValidateOptions = new FluentValidateOptions<TestOptions>(validator.Object);
        var options = new TestOptions();

        // Act
        var result = fluentValidateOptions.Validate(name: null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().NotBeNull();
        result.Failures.Should().HaveCount(1);
        result.FailureMessage.Should().BeEmpty();
        validator.Verify(x => x.Validate(options), Times.Once);
    }

    public sealed class TestOptions { }
}

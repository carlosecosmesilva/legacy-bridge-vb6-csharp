using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using LegacyBridge.Application.Contracts.Responses;
using LegacyBridge.Api.Extensions;

namespace LegacyBridge.UnitTests.Extensions;

public class ApiResponseExtensionsTests
{
    [Fact]
    public void ToActionResult_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var response = ApiResponse<string>.SuccessResult("test data");

        // Act
        var result = response.ToActionResult();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public void ToActionResult_WhenErrorWithNotFound_ReturnsNotFound()
    {
        // Arrange
        var response = ApiResponse<string>.ErrorResult("Customer 123 not found");

        // Act
        var result = response.ToActionResult();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public void ToActionResult_WhenErrorWithNotFoundUpperCase_ReturnsNotFound()
    {
        // Arrange
        var response = ApiResponse<string>.ErrorResult("Product NOT FOUND");

        // Act
        var result = response.ToActionResult();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void ToActionResult_WhenErrorOther_ReturnsBadRequest()
    {
        // Arrange
        var response = ApiResponse<string>.ErrorResult("Validation failed", new List<string> { "Name is required" });

        // Act
        var result = response.ToActionResult();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ToActionResult_WhenErrorWithNullMessage_ReturnsBadRequest()
    {
        // Arrange
        var response = new ApiResponse<string>
        {
            Success = false,
            Message = null,
            Data = default
        };

        // Act
        var result = response.ToActionResult();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}

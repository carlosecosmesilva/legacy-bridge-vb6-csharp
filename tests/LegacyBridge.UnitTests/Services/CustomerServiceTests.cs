using Moq;
using Xunit;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using LegacyBridge.Domain.Interfaces.Repositories;
using LegacyBridge.Application.Services;
using LegacyBridge.Application.Mappings;
using LegacyBridge.Domain.Entities;
using LegacyBridge.Application.DTOs;
using LegacyBridge.Application.Contracts.Requests;

namespace LegacyBridge.UnitTests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock = new();
    private readonly Mock<ILogger<CustomerService>> _loggerMock = new();
    private readonly IMapper _mapper;

    public CustomerServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CustomerProfile>();
            cfg.AddProfile<ProductProfile>();
        });
        _mapper = config.CreateMapper();
    }

    private CustomerService CreateSut() => new(_repoMock.Object, _loggerMock.Object, _mapper);

    [Fact]
    public async Task GetByIdAsync_WhenIdInvalid_ReturnsError()
    {
        // arrange
        var sut = CreateSut();

        // act
        var result = await sut.GetByIdAsync(0);

        // assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid customer ID");
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsCustomer()
    {
        // arrange
        var sut = CreateSut();
        var customer = new Customer { Id = 1, Name = "Alice", Document = "123", Active = true, CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        // act
        var result = await sut.GetByIdAsync(1);

        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data!.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task CreateAsync_WhenInvalidPayload_ReturnsValidationErrors()
    {
        // arrange
        var sut = CreateSut();
        var dto = new CustomerDto { Name = "", Document = new string('9', 21), Status = false };

        // act
        var result = await sut.CreateAsync(dto);

        // assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.Errors.Should().Contain(e => e.Contains("Name is required"));
        result.Errors.Should().Contain(e => e.Contains("Document must be less than 20"));
        result.Errors.Should().Contain(e => e.Contains("Status is required"));
    }

    [Fact]
    public async Task SearchByNameAsync_WhenLimitOutOfRange_ReturnsError()
    {
        // arrange
        var sut = CreateSut();
        var req = new CustomerSearchRequest { Term = "a", Limit = 0, Offset = 0 };

        // act
        var result = await sut.SearchByNameAsync(req);

        // assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Limit must be between 1 and 1000");
    }
}

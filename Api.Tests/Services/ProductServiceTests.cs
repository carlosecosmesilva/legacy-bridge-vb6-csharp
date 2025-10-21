using Api.DTOs;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using Api.Mappings;
using Xunit;

namespace Api.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();
        private readonly Mock<ILogger<ProductService>> _loggerMock = new();
        private readonly IMapper _mapper;

        public ProductServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<CustomerProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private ProductService CreateSut() => new(_repoMock.Object, _loggerMock.Object, _mapper);

        // Add your test methods here
        [Fact]
        public async Task GetByIdAsync_WhenIdInvalid_ReturnsError()
        {
            // arrange
            var sut = CreateSut();
            // act
            var result = await sut.GetByIdAsync(0);
            // assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid product ID");
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsProduct()
        {
            // arrange
            var sut = CreateSut();
            var product = new Product { Id = 1, Name = "Product1", Active = true, Price = 10.0m, CreatedAt = DateTime.UtcNow };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            // act
            var result = await sut.GetByIdAsync(1);
            // assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
            result.Data!.Name.Should().Be("Product1");
        }

        [Fact]
        public async Task CreateAsync_WhenInvalidPayload_ReturnsValidationErrors()
        {
            // arrange
            var sut = CreateSut();
            var dto = new ProductDto { Name = "", Price = -5.0m, Active = true };
            // act
            var result = await sut.CreateAsync(dto);
            // assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Validation errors occurred");
            result.Errors.Should().Contain("Name is required");
            result.Errors.Should().Contain("Price must be greater than zero");
        }

        [Fact]
        public async Task CreateAsync_WhenValidPayload_CreatesProduct()
        {
            // arrange
            var sut = CreateSut();
            var dto = new ProductDto { Name = "New Product", Price = 20.0m, Active = true };
            var createdProduct = new Product { Id = 1, Name = "New Product", Price = 20.0m, Active = true, CreatedAt = DateTime.UtcNow };
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);
            // act
            var result = await sut.CreateAsync(dto);
            // assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
            result.Data!.Name.Should().Be("New Product");
        }

        [Fact]
        public async Task DeleteAsync_WhenIdInvalid_ReturnsError()
        {
            // arrange
            var sut = CreateSut();
            // act
            var result = await sut.DeleteAsync(0);
            // assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid product ID");
        }

        [Fact]
        public async Task DeleteAsync_WhenProductNotFound_ReturnsError()
        {
            // arrange
            var sut = CreateSut();
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);
            // act
            var result = await sut.DeleteAsync(1);
            // assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Product 1 not found");
        }

        [Fact]
        public async Task DeleteAsync_WhenProductDeleted_ReturnsSuccess()
        {
            // arrange
            var sut = CreateSut();
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            // act
            var result = await sut.DeleteAsync(1);
            // assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Product deleted successfully");
        }
    }
}

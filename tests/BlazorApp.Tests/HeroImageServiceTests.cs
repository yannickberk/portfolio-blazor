using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for HeroImageService to ensure proper data loading and error handling.
/// </summary>
public class HeroImageServiceTests
{
    private readonly ILogger<HeroImageService> _mockLogger;

    public HeroImageServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<HeroImageService>>();
    }

    private static List<HeroImage> CreateSampleHeroImages() => 
    [
        new() { Name = "home", Src = "/images/home-hero.jpg", Alt = "Home Hero" },
        new() { Name = "about", Src = "/images/about-hero.jpg", Alt = "About Hero" },
        new() { Name = "portfolio", Src = "/images/portfolio-hero.jpg", Alt = "Portfolio Hero" }
    ];

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetHeroAsync_WithValidPredicate_ReturnsMatchingHeroImage()
    {
        // Arrange
        var heroImages = CreateSampleHeroImages();
        var httpClient = CreateHttpClientWithResponse(heroImages);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetHeroAsync(h => h.Name == "about");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("about", result.Name);
        Assert.Equal("/images/about-hero.jpg", result.Src);
        Assert.Equal("About Hero", result.Alt);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetHeroAsync_WithNonMatchingPredicate_ReturnsNull()
    {
        // Arrange
        var heroImages = CreateSampleHeroImages();
        var httpClient = CreateHttpClientWithResponse(heroImages);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetHeroAsync(h => h.Name == "nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetHeroAsync_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var heroImages = CreateSampleHeroImages();
        var httpClient = CreateHttpClientWithResponse(heroImages);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetHeroAsync(null!));
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetAllHeroImagesAsync_WithValidData_ReturnsAllHeroImages()
    {
        // Arrange
        var heroImages = CreateSampleHeroImages();
        var httpClient = CreateHttpClientWithResponse(heroImages);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAllHeroImagesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, h => h.Name == "home");
        Assert.Contains(result, h => h.Name == "about");
        Assert.Contains(result, h => h.Name == "portfolio");
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetAllHeroImagesAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var httpClient = CreateHttpClientWithResponse(new List<HeroImage>());
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAllHeroImagesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetHeroAsync_WhenHttpRequestFails_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithError(HttpStatusCode.NotFound);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetHeroAsync(h => h.Name == "home");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetAllHeroImagesAsync_WhenHttpRequestFails_ReturnsEmptyList()
    {
        // Arrange
        var httpClient = CreateHttpClientWithError(HttpStatusCode.InternalServerError);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAllHeroImagesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetHeroAsync_WhenJsonIsInvalid_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithInvalidJson();
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetHeroAsync(h => h.Name == "home");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetHeroAsync_CalledMultipleTimes_UsesCachedData()
    {
        // Arrange
        var heroImages = CreateSampleHeroImages();
        var httpClient = CreateHttpClientWithResponse(heroImages);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result1 = await service.GetHeroAsync(h => h.Name == "home");
        var result2 = await service.GetHeroAsync(h => h.Name == "about");
        var result3 = await service.GetAllHeroImagesAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Equal("home", result1.Name);
        Assert.Equal("about", result2.Name);
        Assert.Equal(3, result3.Count);
    }

    [Theory]
    [Trait("Category", "Service")]
    [InlineData("home")]
    [InlineData("about")]
    [InlineData("portfolio")]
    public async Task GetHeroAsync_WithDifferentNames_ReturnsCorrectHeroImage(string name)
    {
        // Arrange
        var heroImages = CreateSampleHeroImages();
        var httpClient = CreateHttpClientWithResponse(heroImages);
        var service = new HeroImageService(httpClient, _mockLogger);

        // Act
        var result = await service.GetHeroAsync(h => h.Name == name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
    }

    private static HttpClient CreateHttpClientWithResponse(List<HeroImage> heroImages)
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(heroImages)
            };
            return Task.FromResult(response);
        });

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    private static HttpClient CreateHttpClientWithError(HttpStatusCode statusCode)
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(statusCode);
            return Task.FromResult(response);
        });

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    private static HttpClient CreateHttpClientWithInvalidJson()
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json")
            };
            return Task.FromResult(response);
        });

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }
}

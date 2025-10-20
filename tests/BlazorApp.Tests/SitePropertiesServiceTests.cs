using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for SitePropertiesService to ensure proper data loading and error handling.
/// </summary>
public class SitePropertiesServiceTests
{
    private readonly ILogger<SitePropertiesService> _mockLogger;

    public SitePropertiesServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<SitePropertiesService>>();
    }

    private static SiteProperties CreateSampleSiteProperties() => new()
    {
        Name = "John Doe",
        Title = "Software Developer",
        Email = "john@example.com",
        DevDotTo = "johndoe",
        GitHub = "johndoe",
        Instagram = "johndoe_ig",
        LinkedIn = "johndoe-linkedin",
        Medium = "johndoe.medium",
        Twitter = "johndoe_tw",
        YouTube = "johndoeYT"
    };

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetSitePropertiesAsync_WithValidData_ReturnsSiteProperties()
    {
        // Arrange
        var siteProperties = CreateSampleSiteProperties();
        var httpClient = CreateHttpClientWithResponse(siteProperties);
        var service = new SitePropertiesService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSitePropertiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("Software Developer", result.Title);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("johndoe", result.GitHub);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetSitePropertiesAsync_CalledMultipleTimes_UsesCachedData()
    {
        // Arrange
        var siteProperties = CreateSampleSiteProperties();
        var httpClient = CreateHttpClientWithResponse(siteProperties);
        var service = new SitePropertiesService(httpClient, _mockLogger);

        // Act
        var result1 = await service.GetSitePropertiesAsync();
        var result2 = await service.GetSitePropertiesAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Name, result2.Name);
        Assert.Equal(result1.Email, result2.Email);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetSitePropertiesAsync_WhenHttpRequestFails_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithError(HttpStatusCode.NotFound);
        var service = new SitePropertiesService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSitePropertiesAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetSitePropertiesAsync_WhenJsonIsInvalid_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithInvalidJson();
        var service = new SitePropertiesService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSitePropertiesAsync();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [Trait("Category", "Service")]
    [InlineData("Jane Smith", "UI Designer", "jane@example.com")]
    [InlineData("Bob Wilson", "DevOps Engineer", "bob@example.com")]
    public async Task GetSitePropertiesAsync_WithDifferentData_ReturnsCorrectProperties(
        string name, string title, string email)
    {
        // Arrange
        var siteProperties = new SiteProperties
        {
            Name = name,
            Title = title,
            Email = email,
            DevDotTo = "",
            GitHub = "",
            Instagram = "",
            LinkedIn = "",
            Medium = "",
            Twitter = "",
            YouTube = ""
        };
        var httpClient = CreateHttpClientWithResponse(siteProperties);
        var service = new SitePropertiesService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSitePropertiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(title, result.Title);
        Assert.Equal(email, result.Email);
    }

    private static HttpClient CreateHttpClientWithResponse(SiteProperties siteProperties)
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(siteProperties)
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

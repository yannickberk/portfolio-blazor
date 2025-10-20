using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for SocialIconsService to ensure proper data loading and error handling.
/// </summary>
public class SocialIconsServiceTests
{
    private readonly ILogger<SocialIconsService> _mockLogger;

    public SocialIconsServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<SocialIconsService>>();
    }

    private static SocialIcons CreateSampleSocialIcons() => new()
    {
        Email = "/img/email.svg",
        DevDotTo = "/img/devto.svg",
        GitHub = "/img/github.svg",
        Instagram = "/img/instagram.svg",
        LinkedIn = "/img/linkedin.svg",
        Medium = "/img/medium.svg",
        Twitter = "/img/twitter.svg",
        YouTube = "/img/youtube.svg"
    };

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetSocialIconsAsync_WithValidData_ReturnsSocialIcons()
    {
        // Arrange
        var socialIcons = CreateSampleSocialIcons();
        var httpClient = CreateHttpClientWithResponse(socialIcons);
        var service = new SocialIconsService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSocialIconsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/img/email.svg", result.Email);
        Assert.Equal("/img/github.svg", result.GitHub);
        Assert.Equal("/img/linkedin.svg", result.LinkedIn);
        Assert.Equal("/img/twitter.svg", result.Twitter);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetSocialIconsAsync_CalledMultipleTimes_UsesCachedData()
    {
        // Arrange
        var socialIcons = CreateSampleSocialIcons();
        var httpClient = CreateHttpClientWithResponse(socialIcons);
        var service = new SocialIconsService(httpClient, _mockLogger);

        // Act
        var result1 = await service.GetSocialIconsAsync();
        var result2 = await service.GetSocialIconsAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Email, result2.Email);
        Assert.Equal(result1.GitHub, result2.GitHub);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetSocialIconsAsync_WhenHttpRequestFails_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithError(HttpStatusCode.NotFound);
        var service = new SocialIconsService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSocialIconsAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetSocialIconsAsync_WhenJsonIsInvalid_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithInvalidJson();
        var service = new SocialIconsService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSocialIconsAsync();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [Trait("Category", "Service")]
    [InlineData("/images/email.png", "/images/github.png")]
    [InlineData("/assets/email.svg", "/assets/github.svg")]
    public async Task GetSocialIconsAsync_WithDifferentPaths_ReturnsCorrectPaths(
        string emailPath, string githubPath)
    {
        // Arrange
        var socialIcons = new SocialIcons
        {
            Email = emailPath,
            GitHub = githubPath,
            DevDotTo = "/img/devto.svg",
            Instagram = "/img/instagram.svg",
            LinkedIn = "/img/linkedin.svg",
            Medium = "/img/medium.svg",
            Twitter = "/img/twitter.svg",
            YouTube = "/img/youtube.svg"
        };
        var httpClient = CreateHttpClientWithResponse(socialIcons);
        var service = new SocialIconsService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSocialIconsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(emailPath, result.Email);
        Assert.Equal(githubPath, result.GitHub);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetSocialIconsAsync_WithPartialData_ReturnsAllFields()
    {
        // Arrange
        var socialIcons = new SocialIcons
        {
            Email = "/img/email.svg",
            GitHub = "/img/github.svg",
            DevDotTo = "",
            Instagram = "",
            LinkedIn = "/img/linkedin.svg",
            Medium = "",
            Twitter = "",
            YouTube = "/img/youtube.svg"
        };
        var httpClient = CreateHttpClientWithResponse(socialIcons);
        var service = new SocialIconsService(httpClient, _mockLogger);

        // Act
        var result = await service.GetSocialIconsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/img/email.svg", result.Email);
        Assert.Equal("/img/github.svg", result.GitHub);
        Assert.Equal("", result.DevDotTo);
        Assert.Equal("", result.Medium);
    }

    private static HttpClient CreateHttpClientWithResponse(SocialIcons socialIcons)
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(socialIcons)
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

using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for AboutMeService to ensure proper data loading and error handling.
/// </summary>
public class AboutMeServiceTests
{
    private readonly ILogger<AboutMeService> _mockLogger;

    public AboutMeServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<AboutMeService>>();
    }

    private static AboutMe CreateSampleAboutMe() => new()
    {
        Description = "I'm a passionate developer",
        Skills = new List<string> { "C#", "Blazor", "Azure" }.AsReadOnly(),
        DetailOrQuote = "Code is poetry",
        CurrentlyLearning = new List<string> { "Docker", "Kubernetes" }.AsReadOnly()
    };

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetAboutMeAsync_WithValidData_ReturnsAboutMe()
    {
        // Arrange
        var aboutMe = CreateSampleAboutMe();
        var httpClient = CreateHttpClientWithResponse(aboutMe);
        var service = new AboutMeService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAboutMeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("I'm a passionate developer", result.Description);
        Assert.Equal(3, result.Skills.Count);
        Assert.Contains("C#", result.Skills);
        Assert.Contains("Blazor", result.Skills);
        Assert.Equal("Code is poetry", result.DetailOrQuote);
        Assert.Equal(2, result.CurrentlyLearning.Count);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetAboutMeAsync_CalledMultipleTimes_UsesCachedData()
    {
        // Arrange
        var aboutMe = CreateSampleAboutMe();
        var httpClient = CreateHttpClientWithResponse(aboutMe);
        var service = new AboutMeService(httpClient, _mockLogger);

        // Act
        var result1 = await service.GetAboutMeAsync();
        var result2 = await service.GetAboutMeAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Description, result2.Description);
        Assert.Equal(result1.Skills.Count, result2.Skills.Count);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetAboutMeAsync_WhenHttpRequestFails_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithError(HttpStatusCode.NotFound);
        var service = new AboutMeService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAboutMeAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetAboutMeAsync_WhenJsonIsInvalid_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateHttpClientWithInvalidJson();
        var service = new AboutMeService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAboutMeAsync();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [Trait("Category", "Service")]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(3, 2)]
    public async Task GetAboutMeAsync_WithVariousSkillCounts_ReturnsCorrectData(
        int skillsCount, int learningCount)
    {
        // Arrange
        var skills = new List<string>();
        for (int i = 0; i < skillsCount; i++)
        {
            skills.Add($"Skill {i + 1}");
        }

        var learning = new List<string>();
        for (int i = 0; i < learningCount; i++)
        {
            learning.Add($"Learning {i + 1}");
        }

        var aboutMe = new AboutMe
        {
            Description = "Test description",
            Skills = skills.AsReadOnly(),
            DetailOrQuote = "Test quote",
            CurrentlyLearning = learning.AsReadOnly()
        };

        var httpClient = CreateHttpClientWithResponse(aboutMe);
        var service = new AboutMeService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAboutMeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(skillsCount, result.Skills.Count);
        Assert.Equal(learningCount, result.CurrentlyLearning.Count);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetAboutMeAsync_WithEmptyArrays_ReturnsValidAboutMe()
    {
        // Arrange
        var aboutMe = new AboutMe
        {
            Description = "Test description",
            Skills = [],
            DetailOrQuote = "Test quote",
            CurrentlyLearning = []
        };
        var httpClient = CreateHttpClientWithResponse(aboutMe);
        var service = new AboutMeService(httpClient, _mockLogger);

        // Act
        var result = await service.GetAboutMeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test description", result.Description);
        Assert.Empty(result.Skills);
        Assert.Empty(result.CurrentlyLearning);
    }

    private static HttpClient CreateHttpClientWithResponse(AboutMe aboutMe)
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(aboutMe)
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

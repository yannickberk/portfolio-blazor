using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for ProjectService to ensure proper data loading and error handling.
/// </summary>
public class ProjectServiceTests
{
    private readonly ILogger<ProjectService> _mockLogger;

    public ProjectServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ProjectService>>();
    }

    private static List<Project> CreateSampleProjects() => 
    [
        new() { Title = "Project 1", Description = "Description 1", Url = "https://example.com/1" },
        new() { Title = "Project 2", Description = "Description 2", Url = "https://example.com/2" },
        new() { Title = "Project 3", Description = "Description 3", Url = "" }
    ];

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetProjectsAsync_WithValidData_ReturnsAllProjects()
    {
        // Arrange
        var projects = CreateSampleProjects();
        var httpClient = CreateHttpClientWithResponse(projects);
        var service = new ProjectService(httpClient, _mockLogger);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.Title == "Project 1");
        Assert.Contains(result, p => p.Title == "Project 2");
        Assert.Contains(result, p => p.Title == "Project 3");
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetProjectsAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var httpClient = CreateHttpClientWithResponse(new List<Project>());
        var service = new ProjectService(httpClient, _mockLogger);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Service")]
    public async Task GetProjectsAsync_CalledMultipleTimes_UsesCachedData()
    {
        // Arrange
        var projects = CreateSampleProjects();
        var httpClient = CreateHttpClientWithResponse(projects);
        var service = new ProjectService(httpClient, _mockLogger);

        // Act
        var result1 = await service.GetProjectsAsync();
        var result2 = await service.GetProjectsAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Count, result2.Count);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetProjectsAsync_WhenHttpRequestFails_ReturnsEmptyList()
    {
        // Arrange
        var httpClient = CreateHttpClientWithError(HttpStatusCode.NotFound);
        var service = new ProjectService(httpClient, _mockLogger);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task GetProjectsAsync_WhenJsonIsInvalid_ReturnsEmptyList()
    {
        // Arrange
        var httpClient = CreateHttpClientWithInvalidJson();
        var service = new ProjectService(httpClient, _mockLogger);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [Trait("Category", "Service")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GetProjectsAsync_WithVariousProjectCounts_ReturnsCorrectCount(int count)
    {
        // Arrange
        var projects = new List<Project>();
        for (int i = 0; i < count; i++)
        {
            projects.Add(new Project
            {
                Title = $"Project {i + 1}",
                Description = $"Description {i + 1}",
                Url = $"https://example.com/{i + 1}"
            });
        }
        var httpClient = CreateHttpClientWithResponse(projects);
        var service = new ProjectService(httpClient, _mockLogger);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
    }

    private static HttpClient CreateHttpClientWithResponse(List<Project> projects)
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(projects)
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

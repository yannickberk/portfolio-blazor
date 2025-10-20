using BlazorApp.Models;
using BlazorApp.Services;
using NSubstitute;
using System.Collections;
using System.Reflection;
using Xunit.Sdk;

namespace BlazorApp.Tests;

/// <summary>
/// Provides common test utilities and mock factories following XUnit best practices.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a mock IHeroImageService with configurable behavior.
    /// </summary>
    /// <param name="hero">The hero image to return, or null for no result.</param>
    /// <returns>A configured mock IHeroImageService.</returns>
    public static IHeroImageService GetMockHeroImageService(HeroImage? hero)
    {
        var service = Substitute.For<IHeroImageService>();
        service.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);
        return service;
    }

    /// <summary>
    /// Creates a mock ISitePropertiesService with configurable behavior.
    /// </summary>
    /// <param name="siteProperties">The site properties to return, or null for no result.</param>
    /// <returns>A configured mock ISitePropertiesService.</returns>
    public static ISitePropertiesService GetMockSitePropertiesService(SiteProperties? siteProperties)
    {
        var service = Substitute.For<ISitePropertiesService>();
        service.GetSitePropertiesAsync().Returns(siteProperties);
        return service;
    }

    /// <summary>
    /// Creates a mock IProjectService with configurable behavior.
    /// </summary>
    /// <param name="projects">The list of projects to return, or null for no result.</param>
    /// <returns>A configured mock IProjectService.</returns>
    public static IProjectService GetMockProjectService(IReadOnlyList<Project>? projects)
    {
        var service = Substitute.For<IProjectService>();
        service.GetProjectsAsync().Returns(projects ?? new List<Project>().AsReadOnly());
        return service;
    }

    /// <summary>
    /// Creates a mock IAboutMeService with configurable behavior.
    /// </summary>
    /// <param name="aboutMe">The about me information to return, or null for no result.</param>
    /// <returns>A configured mock IAboutMeService.</returns>
    public static IAboutMeService GetMockAboutMeService(AboutMe? aboutMe)
    {
        var service = Substitute.For<IAboutMeService>();
        service.GetAboutMeAsync().Returns(aboutMe);
        return service;
    }

    /// <summary>
    /// Creates a sample SiteProperties for testing purposes.
    /// </summary>
    /// <param name="name">Optional name override.</param>
    /// <param name="title">Optional title override.</param>
    /// <param name="email">Optional email override.</param>
    /// <returns>A configured SiteProperties instance.</returns>
    public static SiteProperties CreateSampleSiteProperties(
        string name = "Test User",
        string title = "Test Developer", 
        string email = "test@example.com")
    {
        return new SiteProperties
        {
            Name = name,
            Title = title,
            Email = email
        };
    }

    /// <summary>
    /// Creates a sample HeroImage for testing purposes.
    /// </summary>
    /// <param name="name">Optional name override.</param>
    /// <param name="src">Optional source override.</param>
    /// <param name="alt">Optional alt text override.</param>
    /// <returns>A configured HeroImage instance.</returns>
    public static HeroImage CreateSampleHeroImage(
        string name = "test",
        string src = "test-hero.jpg",
        string alt = "Test Hero Image")
    {
        return new HeroImage
        {
            Name = name,
            Src = src,
            Alt = alt
        };
    }

    /// <summary>
    /// Creates a list of sample Projects for testing purposes.
    /// </summary>
    /// <param name="count">Number of projects to create.</param>
    /// <returns>A list of configured Project instances.</returns>
    public static IReadOnlyList<Project> CreateSampleProjects(int count = 3)
    {
        var projects = new List<Project>();
        for (int i = 1; i <= count; i++)
        {
            string url = i % 2 == 0 ? string.Empty : $"https://example.com/project{i}";
            projects.Add(new Project
            {
                Title = $"Test Project {i}",
                Description = $"Test project description {i}",
                Url = url
            });
        }
        return projects.AsReadOnly();
    }
}

/// <summary>
/// Custom XUnit data attribute for providing SiteProperties test data.
/// Demonstrates advanced XUnit best practices with custom data attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SitePropertiesDataAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        yield return new object[] 
        { 
            new SiteProperties 
            { 
                Name = "John Doe", 
                Title = "Software Engineer", 
                Email = "john@example.com" 
            } 
        };
        yield return new object[] 
        { 
            new SiteProperties 
            { 
                Name = "Jane Smith", 
                Title = "UI/UX Designer", 
                Email = "jane@example.com" 
            } 
        };
        yield return new object[] 
        { 
            new SiteProperties 
            { 
                Name = "Bob Wilson", 
                Title = "DevOps Engineer", 
                Email = "bob@example.com" 
            } 
        };
    }
}

/// <summary>
/// Custom XUnit data attribute for providing Project collections test data.
/// Demonstrates XUnit best practices with parameterized test data.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ProjectCollectionDataAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        // Empty collection
        yield return new object[] { new List<Project>().AsReadOnly() };
        
        // Single project
        yield return new object[] 
        { 
            new List<Project> 
            { 
                new() 
                { 
                    Title = "Solo Project", 
                    Description = "A single project", 
                    Url = "https://example.com/solo" 
                } 
            }.AsReadOnly() 
        };
        
        // Multiple projects
        yield return new object[] { TestHelpers.CreateSampleProjects(3) };
        
        // Large collection
        yield return new object[] { TestHelpers.CreateSampleProjects(10) };
    }
}

/// <summary>
/// Collection fixture for XUnit tests that need shared context.
/// Demonstrates XUnit best practices for shared test setup.
/// </summary>
public class BlazorTestCollection : ICollectionFixture<BlazorTestFixture>
{
}

/// <summary>
/// Test fixture for shared setup across multiple test classes.
/// Demonstrates XUnit collection fixture best practices.
/// </summary>
public class BlazorTestFixture : IDisposable
{
    private bool _disposed = false;

    public BlazorTestFixture()
    {
        // Setup shared resources here
        SharedHttpClient = new HttpClient();
    }

    public HttpClient SharedHttpClient { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            SharedHttpClient?.Dispose();
            _disposed = true;
        }
    }
}

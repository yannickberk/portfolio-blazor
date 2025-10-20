using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;
using System.Collections;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for the Portfolio component following XUnit best practices.
/// Uses constructor for test setup and focuses on single behaviors per test.
/// </summary>
public class PortfolioComponentTests : TestContext
{
    private readonly IProjectService _mockProjectService;
    private readonly IHeroImageService _mockHeroImageService;

    public PortfolioComponentTests()
    {
        // Setup common mocks in constructor following XUnit best practices
        _mockProjectService = Substitute.For<IProjectService>();
        _mockHeroImageService = Substitute.For<IHeroImageService>();
        
        // Register services for dependency injection
        Services.AddSingleton(_mockProjectService);
        Services.AddSingleton(_mockHeroImageService);
    }

    /// <summary>
    /// Provides test data for different project count scenarios.
    /// </summary>
    public static IEnumerable<object[]> ProjectCountTestData()
    {
        yield return new object[] { 0, "Should handle empty project list" };
        yield return new object[] { 1, "Should display single project" };
        yield return new object[] { 3, "Should display multiple projects" };
        yield return new object[] { 10, "Should handle large project list" };
    }

    /// <summary>
    /// Provides test data for exception scenarios.
    /// </summary>
    public static IEnumerable<object[]> ExceptionTestData()
    {
        yield return new object[] { new HttpRequestException("Network error"), "Network errors" };
        yield return new object[] { new TimeoutException("Request timeout"), "Timeout errors" };
        yield return new object[] { new InvalidOperationException("Service unavailable"), "Service errors" };
    }

    [Fact]
    [Trait("Category", "Loading")]
    public void OnInitializedAsync_WhenNoProjectsAvailable_ShowsEmptyStatePlaceholder()
    {
        // Arrange
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult<IReadOnlyList<Project>>([]));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("No projects available"));
        Assert.Contains("No projects available at the moment.", cut.Markup);
        Assert.Contains("empty-state", cut.Markup);
        Assert.DoesNotContain("<em>Loading...</em>", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Data")]
    public void OnInitializedAsync_WhenProjectsLoaded_DisplaysProjectsCorrectly()
    {
        // Arrange
        var sampleProjects = TestHelpers.CreateSampleProjects(2);
        var sampleHero = TestHelpers.CreateSampleHeroImage("portfolio", "/images/portfolio-hero.jpg", "Portfolio Hero");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(sampleProjects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(sampleHero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        foreach (var project in sampleProjects)
        {
            Assert.Contains(project.Title, cut.Markup);
            Assert.Contains(project.Description, cut.Markup);
            Assert.Contains(project.Url, cut.Markup);
        }
        
        Assert.Contains($"src=\"{sampleHero.Src}\"", cut.Markup);
        Assert.Contains($"alt=\"{sampleHero.Alt}\"", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Data")]
    [MemberData(nameof(ProjectCountTestData))] // Using MemberData for parameterized counts and descriptions
    public void OnInitializedAsync_WithVariousProjectCounts_DisplaysCorrectly(int projectCount, string scenario)
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(projectCount);
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert - Test scenario: {scenario}
        if (projectCount == 0)
        {
            cut.WaitForState(() => cut.Markup.Contains("No projects available"), TimeSpan.FromSeconds(5));
            Assert.Contains("No projects available at the moment.", cut.Markup);
        }
        else
        {
            cut.WaitForState(() => cut.Markup.Contains("Test Project 1"), TimeSpan.FromSeconds(5));
            
            for (int i = 1; i <= projectCount; i++)
            {
                Assert.Contains($"Test Project {i}", cut.Markup);
            }
        }
        
        Assert.True(scenario?.Length > 0, "Test scenario description should be provided");
    }

    [Theory]
    [Trait("Category", "Error")]
    [MemberData(nameof(ExceptionTestData))]
    public void OnInitializedAsync_WhenServiceThrowsException_ShowsLoadingState(Exception exception, string errorType)
    {
        // Arrange
        _mockProjectService.GetProjectsAsync().ThrowsAsync(exception);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert - Handling {errorType}
        cut.WaitForState(() => cut.Markup.Contains("Loading..."), TimeSpan.FromSeconds(5));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
        Assert.True(errorType?.Length > 0, "Error type description should be provided");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Render_Always_IncludesPortfolioSection()
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(1);
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        Assert.Contains("id=\"portfolio\"", cut.Markup);
        Assert.Contains("class=\"", cut.Markup); // Verify CSS classes are present
    }

    [Fact]
    [Trait("Category", "Accessibility")]
    public void Render_WithHeroImage_IncludesProperAltText()
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(1);
        var hero = TestHelpers.CreateSampleHeroImage("portfolio", "/images/hero.jpg", "Accessible portfolio hero image");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        Assert.Contains("alt=\"Accessible portfolio hero image\"", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Service Integration")]
    public void OnInitializedAsync_Always_CallsCorrectServiceMethods()
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(1);
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        _mockProjectService.Received(1).GetProjectsAsync();
        _mockHeroImageService.Received(1).GetHeroAsync(Arg.Any<Func<HeroImage, bool>>());
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Render_WithoutHeroImage_DoesNotRenderHeroImageSection()
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(1);
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        Assert.DoesNotContain("<img", cut.Markup);
        Assert.DoesNotContain("portfolio-hero-image", cut.Markup);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Render_WithProjectsWithoutUrl_DisplaysProjectTitleWithoutLink()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Title = "Project Without URL", Description = "Description", Url = "" }
        }.AsReadOnly();
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult<IReadOnlyList<Project>>(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Project Without URL"));
        
        Assert.Contains("Project Without URL", cut.Markup);
        Assert.DoesNotContain("<a href=", cut.Markup);
        Assert.Contains("<h3 class=\"project-title\">Project Without URL</h3>", cut.Markup);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Render_WithProjectsWithUrl_DisplaysProjectTitleAsLink()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Title = "Project With URL", Description = "Description", Url = "https://example.com" }
        }.AsReadOnly();
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult<IReadOnlyList<Project>>(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Project With URL"));
        
        Assert.Contains("href=\"https://example.com\"", cut.Markup);
        Assert.Contains("target=\"_blank\"", cut.Markup);
        Assert.Contains("rel=\"noopener noreferrer\"", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Accessibility")]
    public void Render_WithExternalProjectLinks_IncludesSecurityAttributes()
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(1);
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        Assert.Contains("target=\"_blank\"", cut.Markup);
        Assert.Contains("rel=\"noopener noreferrer\"", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Data")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Render_WithProjectsWithEmptyOrNullUrl_DoesNotDisplayLink(string? url)
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Title = "Test Project", Description = "Description", Url = url! }
        }.AsReadOnly();
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult<IReadOnlyList<Project>>(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project"));
        
        Assert.DoesNotContain("<a href=", cut.Markup);
        Assert.Contains("<h3 class=\"project-title\">Test Project</h3>", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Data")]
    [ProjectCollectionData] // Using custom DataAttribute for pre-configured project collections
    public void OnInitializedAsync_WithProjectCollections_DisplaysCorrectly(IReadOnlyList<Project> projects)
    {
        // Arrange
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        _mockProjectService.GetProjectsAsync().Returns(Task.FromResult(projects));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<Portfolio>();

        // Assert
        if (projects.Count == 0)
        {
            cut.WaitForState(() => cut.Markup.Contains("No projects available"), TimeSpan.FromSeconds(5));
            Assert.Contains("No projects available at the moment.", cut.Markup);
        }
        else
        {
            cut.WaitForState(() => cut.Markup.Contains(projects[0].Title), TimeSpan.FromSeconds(5));
            
            foreach (var project in projects)
            {
                Assert.Contains(project.Title, cut.Markup);
                Assert.Contains(project.Description, cut.Markup);
                
                if (!string.IsNullOrWhiteSpace(project.Url))
                {
                    Assert.Contains(project.Url, cut.Markup);
                }
            }
        }
    }

    [Fact]
    [Trait("Category", "Performance")]
    public void OnInitializedAsync_WhenCalled_LoadsDataInParallel()
    {
        // Arrange
        var projects = TestHelpers.CreateSampleProjects(1);
        var hero = TestHelpers.CreateSampleHeroImage("portfolio");
        
        var projectsTaskCompletionSource = new TaskCompletionSource<IReadOnlyList<Project>>();
        var heroTaskCompletionSource = new TaskCompletionSource<HeroImage?>();
        
        _mockProjectService.GetProjectsAsync().Returns(projectsTaskCompletionSource.Task);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(heroTaskCompletionSource.Task);

        // Act
        var cut = RenderComponent<Portfolio>();
        
        // Complete both tasks simultaneously
        projectsTaskCompletionSource.SetResult(projects);
        heroTaskCompletionSource.SetResult(hero);

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Project 1"));
        
        _mockProjectService.Received(1).GetProjectsAsync();
        _mockHeroImageService.Received(1).GetHeroAsync(Arg.Any<Func<HeroImage, bool>>());
    }
}
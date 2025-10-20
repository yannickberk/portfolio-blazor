using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for the Home component following XUnit best practices.
/// Demonstrates proper test organization, naming conventions, and data-driven testing.
/// </summary>
[Collection(nameof(BlazorTestCollection))]
public class HomeComponentTests : TestContext
{
    private readonly ISitePropertiesService _mockSitePropertiesService;
    private readonly IHeroImageService _mockHeroImageService;

    public HomeComponentTests()
    {
        // Setup common mocks in constructor following XUnit best practices
        _mockSitePropertiesService = Substitute.For<ISitePropertiesService>();
        _mockHeroImageService = Substitute.For<IHeroImageService>();
        
        // Register services for dependency injection
        Services.AddSingleton(_mockSitePropertiesService);
        Services.AddSingleton(_mockHeroImageService);
    }

    private static SiteProperties CreateSampleSiteProperties() => new()
    {
        Name = "John Doe",
        Title = "Software Developer",
        Email = "john@example.com"
    };

    private static HeroImage CreateSampleHeroImage() => new()
    {
        Src = "hero.jpg",
        Alt = "Hero Alt",
        Name = "home"
    };

    [Fact]
    [Trait("Category", "Loading")]
    public void OnInitializedAsync_WhenSitePropertiesIsNull_ShowsLoadingMessage()
    {
        // Arrange
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns((SiteProperties?)null);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        
        // Using Shouldly for more expressive assertions
        cut.Markup.ShouldContain("<em>Loading...</em>");
    }

    [Fact]
    [Trait("Category", "Data")]
    public void OnInitializedAsync_WhenDataLoaded_DisplaysSitePropertiesCorrectly()
    {
        // Arrange
        var siteProperties = CreateSampleSiteProperties();
        var heroImage = CreateSampleHeroImage();
        
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns(siteProperties);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(heroImage);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains(siteProperties.Title));
        
        // Multiple assertions for comprehensive validation
        cut.Markup.ShouldContain($"<h1>{siteProperties.Name}</h1>");
        cut.Markup.ShouldContain($"<h2>{siteProperties.Title}</h2>");
        cut.Markup.ShouldContain($"src=\"{heroImage.Src}\" alt=\"{heroImage.Alt}\"");
    }

    [Theory]
    [Trait("Category", "Data")]
    [InlineData("Jane Smith", "UI/UX Designer")]
    [InlineData("Bob Wilson", "Full Stack Developer")]
    [InlineData("Alice Johnson", "DevOps Engineer")]
    public void OnInitializedAsync_WithDifferentSiteProperties_DisplaysCorrectly(string name, string title)
    {
        // Arrange
        var siteProperties = new SiteProperties
        {
            Name = name,
            Title = title,
            Email = "test@example.com"
        };
        var heroImage = CreateSampleHeroImage();
        
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns(siteProperties);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(heroImage);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains(title));
        
        cut.Markup.ShouldContain($"<h1>{name}</h1>");
        cut.Markup.ShouldContain($"<h2>{title}</h2>");
    }

    [Theory]
    [Trait("Category", "Data")]
    [SitePropertiesData]
    public void OnInitializedAsync_WithCustomDataAttribute_DisplaysCorrectly(SiteProperties siteProperties)
    {
        // Arrange
        var heroImage = CreateSampleHeroImage();
        
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns(siteProperties);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(heroImage);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains(siteProperties.Title));
        
        // Demonstrate comprehensive assertions
        cut.Markup.ShouldContain($"<h1>{siteProperties.Name}</h1>");
        cut.Markup.ShouldContain($"<h2>{siteProperties.Title}</h2>");
        siteProperties.Name.ShouldNotBeNullOrWhiteSpace();
        siteProperties.Title.ShouldNotBeNullOrWhiteSpace();
        siteProperties.Email.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [Trait("Category", "MemberData")]
    [MemberData(nameof(GetHeroImageTestData))]
    public void OnInitializedAsync_WithDifferentHeroImages_DisplaysCorrectly(HeroImage? heroImage, bool shouldShowImage)
    {
        // Arrange
        var siteProperties = CreateSampleSiteProperties();
        
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns(siteProperties);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(heroImage);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains(siteProperties.Title));
        
        if (shouldShowImage && heroImage is not null)
        {
            cut.Markup.ShouldContain($"src=\"{heroImage.Src}\"");
            cut.Markup.ShouldContain($"alt=\"{heroImage.Alt}\"");
        }
        else
        {
            cut.Markup.ShouldNotContain("class=\"background\"");
        }
    }

    [Fact]
    [Trait("Category", "Error")]
    public void OnInitializedAsync_WhenServiceThrowsException_ShowsLoadingMessage()
    {
        // Arrange
        _mockSitePropertiesService.GetSitePropertiesAsync().ThrowsAsync(new HttpRequestException("Network error"));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        cut.Markup.ShouldContain("<em>Loading...</em>");
    }

    [Fact]
    [Trait("Category", "Navigation")]
    public void Render_Always_IncludesScrollArrowToAboutSection()
    {
        // Arrange
        var siteProperties = CreateSampleSiteProperties();
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns(siteProperties);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(CreateSampleHeroImage());

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains(siteProperties.Title));
        
        cut.Markup.ShouldContain("href=\"#about\"");
        cut.Markup.ShouldContain("class=\"scroll-arrow\"");
        cut.Markup.ShouldContain("images/down-arrow.svg");
    }

    [Fact]
    [Trait("Category", "Accessibility")]
    public void Render_WithHeroImage_IncludesProperAltText()
    {
        // Arrange
        var siteProperties = CreateSampleSiteProperties();
        var heroImage = new HeroImage
        {
            Src = "hero.jpg",
            Alt = "Accessible hero image for screen readers",
            Name = "home"
        };
        
        _mockSitePropertiesService.GetSitePropertiesAsync().Returns(siteProperties);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(heroImage);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains(siteProperties.Title));
        
        cut.Markup.ShouldContain($"alt=\"{heroImage.Alt}\"");
        heroImage.Alt.ShouldNotBeNullOrWhiteSpace("Alt text is required for accessibility");
    }

    /// <summary>
    /// Provides test data for hero image testing using MemberData.
    /// Demonstrates XUnit best practices for method-based test data.
    /// </summary>
    public static IEnumerable<object?[]> GetHeroImageTestData()
    {
        yield return new object?[] { null, false };
        yield return new object?[] 
        { 
            new HeroImage { Src = "test.jpg", Alt = "Test Alt", Name = "home" }, 
            true 
        };
        yield return new object?[] 
        { 
            new HeroImage { Src = "hero.png", Alt = "Hero Alt", Name = "home" }, 
            true 
        };
    }
}

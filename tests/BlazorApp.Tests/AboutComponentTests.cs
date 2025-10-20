using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;

namespace BlazorApp.Tests;

public class AboutComponentTests : TestContext
{
    private readonly IAboutMeService _mockAboutMeService;
    private readonly IHeroImageService _mockHeroImageService;

    public AboutComponentTests()
    {
        // Setup common mocks in constructor following XUnit best practices
        _mockAboutMeService = Substitute.For<IAboutMeService>();
        _mockHeroImageService = Substitute.For<IHeroImageService>();
        
        // Register services for dependency injection
        Services.AddSingleton(_mockAboutMeService);
        Services.AddSingleton(_mockHeroImageService);
    }

    private static AboutMe GetSampleAboutMe() => new()
    {
        Description = "Test description",
        Skills = ["C#", "Blazor"],
        DetailOrQuote = "Test quote",
        CurrentlyLearning = ["Docker", "Azure"]
    };

    private static HeroImage GetSampleHero() => new()
    {
        Src = "hero.jpg",
        Alt = "Hero Alt",
        Name = "about"
    };


    [Fact]
    [Trait("Category", "Loading")]
    public void Render_WhenAboutMeDataIsNull_ShowsLoadingState()
    {
        // Arrange
        _mockAboutMeService.GetAboutMeAsync().Returns((AboutMe?)null);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Data")]
    public void RenderAsync_WithValidData_RendersAboutMeContentCorrectly()
    {
        // Arrange
        var aboutMe = GetSampleAboutMe();
        _mockAboutMeService.GetAboutMeAsync().Returns(aboutMe);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test description"));
        Assert.Contains("Test description", cut.Markup);
        Assert.Contains("Test quote", cut.Markup);
        Assert.Contains("C#", cut.Markup);
        Assert.Contains("Blazor", cut.Markup);
        Assert.Contains("Currently learning", cut.Markup);
        Assert.Contains("Docker", cut.Markup);
        Assert.Contains("Azure", cut.Markup);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void RenderAsync_WithHeroImageAvailable_RendersHeroImage()
    {
        // Arrange
        var aboutMe = GetSampleAboutMe();
        var hero = GetSampleHero();
        _mockAboutMeService.GetAboutMeAsync().Returns(aboutMe);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("src=\"hero.jpg\""));
        Assert.Contains("src=\"hero.jpg\"", cut.Markup);
        Assert.Contains("alt=\"Hero Alt\"", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Data")]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void RenderAsync_WithEmptySkillsOrLearning_RendersCorrectly(bool hasSkills, bool hasLearning)
    {
        // Arrange
        var aboutMe = new AboutMe
        {
            Description = "Test description",
            Skills = hasSkills ? ["C#"] : [],
            DetailOrQuote = "Test quote",
            CurrentlyLearning = hasLearning ? ["Docker"] : []
        };
        _mockAboutMeService.GetAboutMeAsync().Returns(aboutMe);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test description"));
        
        if (hasSkills)
        {
            Assert.Contains("C#", cut.Markup);
        }
        
        if (hasLearning)
        {
            Assert.Contains("Currently learning", cut.Markup);
            Assert.Contains("Docker", cut.Markup);
        }
        else
        {
            Assert.DoesNotContain("Currently learning", cut.Markup);
        }
    }

    [Fact]
    [Trait("Category", "Error")]
    public void RenderAsync_WhenServiceThrowsException_ShowsLoadingState()
    {
        // Arrange
        _mockAboutMeService.GetAboutMeAsync().ThrowsAsync(new HttpRequestException("Network error"));
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Data")]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("test.jpg", "Test Alt")]
    public void RenderAsync_WithVariousHeroImageData_RendersCorrectly(string? src, string? alt)
    {
        // Arrange
        var aboutMe = GetSampleAboutMe();
        var hero = src is not null ? new HeroImage { Src = src, Alt = alt ?? "", Name = "about" } : null;
        _mockAboutMeService.GetAboutMeAsync().Returns(aboutMe);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test description"));
        
        if (hero is not null)
        {
            Assert.Contains($"src=\"{src}\"", cut.Markup);
            Assert.Contains($"alt=\"{alt}\"", cut.Markup);
        }
        else
        {
            Assert.DoesNotContain("class=\"background\"", cut.Markup);
        }
    }

    [Fact]
    [Trait("Category", "Accessibility")]
    public void RenderAsync_Always_IncludesProperSectionStructure()
    {
        // Arrange
        var aboutMe = GetSampleAboutMe();
        _mockAboutMeService.GetAboutMeAsync().Returns(aboutMe);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test description"));
        Assert.Contains("id=\"about\"", cut.Markup);
        Assert.Contains("<h2>About Myself</h2>", cut.Markup);
        Assert.Contains("class=\"about-content\"", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Service Integration")]
    public void OnInitializedAsync_Always_CallsCorrectServiceMethods()
    {
        // Arrange
        var aboutMe = GetSampleAboutMe();
        _mockAboutMeService.GetAboutMeAsync().Returns(aboutMe);
        _mockHeroImageService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns((HeroImage?)null);

        // Act
        var cut = RenderComponent<About>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test description"));
        _mockAboutMeService.Received(1).GetAboutMeAsync();
        _mockHeroImageService.Received(1).GetHeroAsync(Arg.Any<Func<HeroImage, bool>>());
    }
}

using Bunit;
using Xunit;
using BlazorApp.Shared;
using BlazorApp.Models;
using BlazorApp.Services;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for the Footer component to ensure proper rendering and data handling.
/// </summary>
public class FooterComponentTests : TestContext
{
    private static SiteProperties SampleProperties => new()
    {
        Title = "Jane Doe Portfolio",
        Name = "Jane Doe",
        Email = "jane@example.com",
        DevDotTo = "janedev",
        GitHub = "janedoe",
        Instagram = "janedoe_insta",
        LinkedIn = "janedoe-linkedin",
        Medium = "janedoe.medium",
        Twitter = "janedoe_tw",
        YouTube = "janedoeYT"
    };

    private static SocialIcons SampleIcons => new()
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

    /// <summary>
    /// Configures test services with mocked dependencies.
    /// </summary>
    /// <param name="siteProperties">The site properties to return from the mock service.</param>
    /// <param name="socialIcons">The social icons to return from the mock service.</param>
    private void ConfigureServices(SiteProperties? siteProperties = null, SocialIcons? socialIcons = null)
    {
        var sitePropertiesService = Substitute.For<ISitePropertiesService>();
        var socialIconsService = Substitute.For<ISocialIconsService>();
        var logger = Substitute.For<ILogger<Footer>>();

        sitePropertiesService.GetSitePropertiesAsync().Returns(Task.FromResult(siteProperties));
        socialIconsService.GetSocialIconsAsync().Returns(Task.FromResult(socialIcons));

        Services.AddSingleton(sitePropertiesService);
        Services.AddSingleton(socialIconsService);
        Services.AddSingleton(logger);
    }

    [Fact]
    [Trait("Category", "Loading")]
    public void Render_WhenDataNotLoaded_ShowsLoadingState()
    {
        // Arrange
        ConfigureServices(null, null);

        // Act
        var cut = RenderComponent<Footer>();

        // Assert
        cut.MarkupMatches(@"
<div id=""contact"">
    <div class=""social-icons-container"">
        <p><em>Loading...</em></p>
    </div>
</div>");
    }

    [Fact]
    [Trait("Category", "Data")]
    public async Task RenderAsync_WithValidData_RendersAllSocialIconsAndFooterCredit()
    {
        // Arrange
        ConfigureServices(SampleProperties, SampleIcons);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        var icons = cut.FindAll("img.social-icon");
        Assert.Equal(8, icons.Count);
        Assert.Contains("Created by Jane Doe", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Data")]
    public async Task RenderAsync_WithEmptyAndNullSocialLinks_RendersOnlyValidLinks()
    {
        // Arrange
        var props = new SiteProperties
        {
            Title = SampleProperties.Title,
            Name = SampleProperties.Name,
            Email = SampleProperties.Email,
            DevDotTo = SampleProperties.DevDotTo,
            GitHub = SampleProperties.GitHub,
            Instagram = "janedoe_insta",
            LinkedIn = SampleProperties.LinkedIn,
            Medium = null!,
            Twitter = "",
            YouTube = SampleProperties.YouTube
        };
        ConfigureServices(props, SampleIcons);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        var icons = cut.FindAll("img.social-icon");
        Assert.DoesNotContain(icons, i => i.GetAttribute("alt") == "Twitter");
        Assert.DoesNotContain(icons, i => i.GetAttribute("alt") == "Medium");
        Assert.Contains(icons, i => i.GetAttribute("alt") == "Instagram");
    }

    [Fact]
    [Trait("Category", "Data")]
    public async Task RenderAsync_WhenIconsDataMissing_DoesNotRenderIcons()
    {
        // Arrange
        ConfigureServices(SampleProperties, null);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        Assert.Empty(cut.FindAll("img.social-icon"));
    }

    [Fact]
    [Trait("Category", "Data")]
    public async Task RenderAsync_WhenPropertiesMissing_DoesNotRenderFooterCredit()
    {
        // Arrange
        ConfigureServices(null, SampleIcons);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        Assert.DoesNotContain("footer-credit", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Data")]
    [InlineData("", "", false)]
    [InlineData(null, null, false)]
    [InlineData("John Doe", "john@example.com", true)]
    [InlineData("Jane Smith", "", true)]
    public async Task RenderAsync_WithVariousNameAndEmailCombinations_RendersFooterCreditCorrectly(
        string? name, string? email, bool shouldShowCredit)
    {
        // Arrange
        var properties = name is not null ? new SiteProperties 
        { 
            Name = name, 
            Email = email ?? "",
            Title = "Test",
            DevDotTo = "",
            GitHub = "",
            Instagram = "",
            LinkedIn = "",
            Medium = "",
            Twitter = "",
            YouTube = ""
        } : null;
        ConfigureServices(properties, SampleIcons);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        if (shouldShowCredit && !string.IsNullOrEmpty(name))
        {
            Assert.Contains($"Created by {name}", cut.Markup);
        }
        else
        {
            Assert.DoesNotContain("Created by", cut.Markup);
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task RenderAsync_Always_RendersContactSection()
    {
        // Arrange
        ConfigureServices(SampleProperties, SampleIcons);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        Assert.Contains("id=\"contact\"", cut.Markup);
        Assert.Contains("social-icons-container", cut.Markup);
    }

    [Theory]
    [Trait("Category", "Accessibility")]
    [InlineData("Email", "/img/email.svg")]
    [InlineData("GitHub", "/img/github.svg")]
    [InlineData("LinkedIn", "/img/linkedin.svg")]
    public async Task RenderAsync_WithSocialIcons_IncludesProperAltAttributes(string expectedAlt, string iconSrc)
    {
        // Arrange
        ConfigureServices(SampleProperties, SampleIcons);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        var icons = cut.FindAll("img.social-icon");
        Assert.Contains(icons, icon => 
            icon.GetAttribute("alt") == expectedAlt && 
            icon.GetAttribute("src") == iconSrc);
    }

    [Fact]
    [Trait("Category", "Error Handling")]
    public async Task RenderAsync_WhenServiceThrowsException_ShowsLoadingState()
    {
        // Arrange
        var sitePropertiesService = Substitute.For<ISitePropertiesService>();
        var socialIconsService = Substitute.For<ISocialIconsService>();
        var logger = Substitute.For<ILogger<Footer>>();

        sitePropertiesService.GetSitePropertiesAsync().Returns<SiteProperties?>(_ => throw new InvalidOperationException("Test exception"));
        socialIconsService.GetSocialIconsAsync().Returns<SocialIcons?>(_ => throw new InvalidOperationException("Test exception"));

        Services.AddSingleton(sitePropertiesService);
        Services.AddSingleton(socialIconsService);
        Services.AddSingleton(logger);

        // Act
        var cut = RenderComponent<Footer>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        Assert.Contains("Loading...", cut.Markup);
        Assert.Empty(cut.FindAll("img.social-icon"));
    }
}
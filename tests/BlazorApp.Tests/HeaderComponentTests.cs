using Bunit;
using Xunit;
using BlazorApp.Shared;

namespace BlazorApp.Tests;

public class HeaderComponentTests : TestContext
{
    public HeaderComponentTests()
    {
        JSInterop.SetupVoid("headerInterop.updateVisibleSections", _ => true);
    }

    [Fact]
    public void RendersAllNavigationLinks()
    {
        // Act
        var cut = RenderComponent<Header>();

        // Assert
        var links = cut.FindAll("a.nav-link");
        Assert.Equal(4, links.Count);
        Assert.Equal("#home", links[0].GetAttribute("href"));
        Assert.Equal("#about", links[1].GetAttribute("href"));
        Assert.Equal("#portfolio", links[2].GetAttribute("href"));
        Assert.Equal("#contact", links[3].GetAttribute("href"));

        Assert.Contains("Home", links[0].InnerHtml);
        Assert.Contains("About", links[1].InnerHtml);
        Assert.Contains("Portfolio", links[2].InnerHtml);
        Assert.Contains("Contact", links[3].InnerHtml);
    }

    [Fact]
    public void HomeLinkIsActiveByDefault()
    {
        var cut = RenderComponent<Header>();
        var homeLink = cut.Find("a.nav-link[href='#home']");
        Assert.Contains("active", homeLink.ClassList);
    }

    [Fact]
    public async Task UpdateVisibleSectionsSetsActiveLink()
    {
        var cut = RenderComponent<Header>();
        // Simulate JSInvokable call to set "portfolio" as active
        await cut.Instance.UpdateVisibleSections(["portfolio"]);
        cut.Render();

        var portfolioLink = cut.Find("a.nav-link[href='#portfolio']");
        Assert.Contains("active", portfolioLink.ClassList);

        // Other links should not be active
        var homeLink = cut.Find("a.nav-link[href='#home']");
        Assert.DoesNotContain("active", homeLink.ClassList);
    }

    [Fact]
    public async Task MultipleSectionsActive()
    {
        var cut = RenderComponent<Header>();
        await cut.Instance.UpdateVisibleSections(["about", "portfolio"]);
        cut.Render();

        var aboutLink = cut.Find("a.nav-link[href='#about']");
        var portfolioLink = cut.Find("a.nav-link[href='#portfolio']");
        Assert.Contains("active", aboutLink.ClassList);
        Assert.Contains("active", portfolioLink.ClassList);

        var homeLink = cut.Find("a.nav-link[href='#home']");
        Assert.DoesNotContain("active", homeLink.ClassList);
    }

    [Fact]
    public void DisposeCleansUpDotNetObjectReference()
    {
        var cut = RenderComponent<Header>();
        // Should not throw
        var exception = Record.Exception(cut.Instance.Dispose);
        Assert.Null(exception);
    }
}
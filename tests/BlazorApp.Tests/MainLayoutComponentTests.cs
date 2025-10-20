using Bunit;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Tests;

/// <summary>
/// Tests for MainLayout component to ensure proper rendering.
/// </summary>
public class MainLayoutComponentTests : TestContext
{
    [Fact]
    [Trait("Category", "UI")]
    public void Render_Always_RendersMainDiv()
    {
        // Act
        var cut = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("id=\"main\"", cut.Markup);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Render_WithBody_RendersBodyContent()
    {
        // Arrange
        var bodyContent = "Test Body Content";

        // Act
        var cut = RenderComponent<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder =>
            {
                builder.AddContent(0, bodyContent);
            })));

        // Assert
        Assert.Contains(bodyContent, cut.Markup);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Render_Always_HasCorrectStructure()
    {
        // Act
        var cut = RenderComponent<MainLayout>();

        // Assert
        var mainDiv = cut.Find("#main");
        Assert.NotNull(mainDiv);
    }
}

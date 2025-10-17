using System.Net;
using System.Net.Http.Json;
using Bunit;
using NSubstitute;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;

namespace BlazorApp.Tests;

public class PortfolioComponentTests : TestContext
{
    private static readonly List<Project> SampleProjects = new()
    {
        new Project { Title = "Project 1", Description = "Desc 1", Url = "https://example.com/1" },
        new Project { Title = "Project 2", Description = "Desc 2", Url = null! },
        new Project { Title = "Project 3", Description = "Desc 3", Url = "" }
    };

    private static readonly HeroImage SampleHero = new()
    {
        Name = "portfolio",
        Src = "/img/portfolio-hero.jpg",
        Alt = "Portfolio Hero"
    };

    private static HttpClient CreateMockHttpClient(List<Project>? projects)
    {
        var handler = new DelegatingHandlerStub(async (request, cancellationToken) =>
        {
            if (request.RequestUri!.ToString().Contains("projects.json"))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(projects)
                };
                return response;
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
    }

    [Fact]
    public void ShowsLoadingStateInitially()
    {
        // Arrange: HttpClient returns null (simulate loading)
        var http = CreateMockHttpClient(null);
        var heroService = Substitute.For<IHeroImageService>();

        // Act
        var cut = RenderComponent<Portfolio>(parameters => parameters
            .Add(p => p.Http, http)
            .Add(p => p.HeroImageService, heroService)
        );

        // Assert
        cut.MarkupMatches(@"
<section class=""light"" id=""portfolio"">
    <h2>Portfolio</h2>
    <div class=""portfolio-container"">
        <p><em>Loading...</em></p>
    </div>
</section>");
    }

    [Fact]
    public async Task RendersHeroImageIfPresent()
    {
        // Arrange
        var http = CreateMockHttpClient(SampleProjects);
        var heroService = Substitute.For<IHeroImageService>();
        heroService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>())
            .Returns(Task.FromResult<HeroImage?>(SampleHero));

        // Act
        var cut = RenderComponent<Portfolio>(parameters => parameters
            .Add(p => p.Http, http)
            .Add(p => p.HeroImageService, heroService)
        );

        // Wait for OnInitializedAsync
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        var img = cut.Find("img.portfolio-hero-image");
        Assert.Equal(SampleHero.Src, img.GetAttribute("src"));
        Assert.Equal(SampleHero.Alt, img.GetAttribute("alt"));
    }

    [Fact]
    public async Task RendersAllProjectsWithLinksAndTitles()
    {
        // Arrange
        var http = CreateMockHttpClient(SampleProjects);
        var heroService = Substitute.For<IHeroImageService>();
        heroService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>())
            .Returns(Task.FromResult<HeroImage?>(SampleHero));

        // Act
        var cut = RenderComponent<Portfolio>(parameters => parameters
            .Add(p => p.Http, http)
            .Add(p => p.HeroImageService, heroService)
        );
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        var boxes = cut.FindAll(".box");
        Assert.Equal(3, boxes.Count);

        // First project has a link
        var link = boxes[0].QuerySelector("a");
        Assert.NotNull(link);
        Assert.Equal("https://example.com/1", link!.GetAttribute("href"));
        Assert.Contains("Project 1", link.InnerHtml);

        // Second project has no link
        Assert.Null(boxes[1].QuerySelector("a"));
        Assert.Contains("Project 2", boxes[1].InnerHtml);

        // Third project has no link (empty Url)
        Assert.Null(boxes[2].QuerySelector("a"));
        Assert.Contains("Project 3", boxes[2].InnerHtml);
    }

    [Fact]
    public async Task HandlesEmptyProjectList()
    {
        // Arrange
        var http = CreateMockHttpClient(new List<Project>());
        var heroService = Substitute.For<IHeroImageService>();
        heroService.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>())
            .Returns(Task.FromResult<HeroImage?>(SampleHero));

        // Act
        var cut = RenderComponent<Portfolio>(parameters => parameters
            .Add(p => p.Http, http)
            .Add(p => p.HeroImageService, heroService)
        );
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert: No .box elements
        Assert.Empty(cut.FindAll(".box"));
    }
}
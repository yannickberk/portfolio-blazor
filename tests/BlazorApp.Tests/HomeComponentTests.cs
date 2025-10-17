using System.Net;
using BlazorApp.Components;
using BlazorApp.Models;
using Bunit;

namespace BlazorApp.Tests;

public class HomeComponentTests : TestContext
{
    private static HeroImage GetSampleHero() => new HeroImage
    {
        Src = "hero.jpg",
        Alt = "Hero Alt",
        Name = "home"
    };

    private static HttpClient GetMockHttpClient(SiteProperties? siteProperties)
    {
        var handler = new DelegatingHandlerStub(async (request, cancellationToken) =>
        {
            if (request.RequestUri!.ToString().Contains("sample-data/siteproperties.json"))
            {
                var json = siteProperties is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(siteProperties);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
    }

    [Fact]
    public void RendersLoadingWhenSitePropertiesIsNull()
    {
        var httpClient = GetMockHttpClient(null);
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<Home>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }

    [Fact]
    public void RendersSitePropertiesDataCorrectly()
    {
        var sampleProperties = new SiteProperties
        {
            Title = "Welcome to My Site",
            Name = "Bob Builder",
        };

        var httpClient = GetMockHttpClient(sampleProperties);
        HeroImage hero = GetSampleHero();
        var heroService = TestHelpers.GetMockHeroImageService(hero);

        var cut = RenderComponent<Home>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains(sampleProperties.Title));

        Assert.Contains($"<h1>{sampleProperties.Name}</h1>", cut.Markup);
        Assert.Contains($"<h2>{sampleProperties.Title}</h2>", cut.Markup);
        Assert.Contains($"src=\"{hero.Src}\" alt=\"{hero.Alt}\"", cut.Markup);
    }

    [Fact]
    public void RendersLoadingOnHttpRequestException()
    {
        var handler = new DelegatingHandlerStub((request, cancellationToken) =>
        {
            throw new HttpRequestException("Simulated network error");
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<Home>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }
}

using Bunit;
using System.Net;
using BlazorApp.Components;
using BlazorApp.Models;

namespace BlazorApp.Tests;
public partial class AboutComponentTests : TestContext
{
    private static AboutMe GetSampleAboutMe() => new AboutMe {
        Description = "Test description",
        Skills = ["C#", "Blazor"],
        DetailOrQuote = "Test quote",
        CurrentlyLearning =  ["Docker", "Azure"]
    };

    private static HeroImage GetSampleHero() => new HeroImage {
        Src = "hero.jpg",
        Alt = "Hero Alt",
        Name = "about"
    };

    private static HttpClient GetMockHttpClient(AboutMe? aboutMe)
    {
        var handler = new DelegatingHandlerStub(async (request, cancellationToken) =>
        {
            if (request.RequestUri!.ToString().Contains("sample-data/aboutme.json"))
            {
                var json = aboutMe is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(aboutMe);
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
    public void RendersLoadingWhenAboutMeIsNull()
    {
        var httpClient = GetMockHttpClient(null);
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }

    [Fact]
    public void RendersAboutMeDataCorrectly()
    {
        var httpClient = GetMockHttpClient(GetSampleAboutMe());
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

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
    public void RendersHeroImageWhenAvailable()
    {
        var httpClient = GetMockHttpClient(GetSampleAboutMe());
        var heroService = TestHelpers.GetMockHeroImageService(GetSampleHero());

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("src=\"hero.jpg\""));
        Assert.Contains("src=\"hero.jpg\"", cut.Markup);
        Assert.Contains("alt=\"Hero Alt\"", cut.Markup);
    }

    [Fact]
    public void RendersNoSkillsOrLearningIfEmpty()
    {
        var aboutMe = new AboutMe {
            Description = "Desc",
            Skills = new List<string>(),
            DetailOrQuote = "Quote",
            CurrentlyLearning = new List<string>()
        };
        var httpClient = GetMockHttpClient(aboutMe);
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Desc"));
        Assert.DoesNotContain("<li", cut.Markup); // No skills or learning items
    }

    [Fact]
    public void RendersLoadingOnJsonException()
    {
        // Simulate invalid JSON response
        var handler = new DelegatingHandlerStub(async (request, cancellationToken) =>
        {
            if (request.RequestUri!.ToString().Contains("sample-data/aboutme.json"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("invalid-json", System.Text.Encoding.UTF8, "application/json")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }

    [Fact]
    public void RendersLoadingOnHttpRequestException()
    {
        // Simulate HTTP error
        var handler = new DelegatingHandlerStub(async (request, cancellationToken) =>
        {
            throw new HttpRequestException();
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Loading..."));
        Assert.Contains("<em>Loading...</em>", cut.Markup);
    }

    [Fact]
    public void RendersNoSkillsIfNull()
    {
        var aboutMe = new AboutMe {
            Description = "Desc",
            Skills = null!,
            DetailOrQuote = "Quote",
            CurrentlyLearning = new List<string>()
        };
        var httpClient = GetMockHttpClient(aboutMe);
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Desc"));
        Assert.DoesNotContain("<li", cut.Markup); // No skills
    }

    [Fact]
    public void RendersNoCurrentlyLearningIfNull()
    {
        var aboutMe = new AboutMe {
            Description = "Desc",
            Skills = new List<string> { "C#" },
            DetailOrQuote = "Quote",
            CurrentlyLearning = null!
        };
        var httpClient = GetMockHttpClient(aboutMe);
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Desc"));
        Assert.DoesNotContain("Currently learning", cut.Markup);
    }

    [Fact]
    public void RendersEmptyDetailOrQuote()
    {
        var aboutMe = new AboutMe {
            Description = "Desc",
            Skills = new List<string> { "C#" },
            DetailOrQuote = string.Empty,
            CurrentlyLearning = new List<string> { "Docker" }
        };
        var httpClient = GetMockHttpClient(aboutMe);
        var heroService = TestHelpers.GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Desc"));
        Assert.Contains("class=\"quote\">", cut.Markup); // Should render the quote element even if empty
    }

    [Fact]
    public void RendersHeroImageWithMissingProperties()
    {
        var hero = new HeroImage {
            Src = string.Empty,
            Alt = string.Empty,
            Name = "about"
        };
        var httpClient = GetMockHttpClient(GetSampleAboutMe());
        var heroService = TestHelpers.GetMockHeroImageService(hero);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("<img"));
        Assert.Contains("src=\"\"", cut.Markup);
        Assert.Contains("alt=\"\"", cut.Markup);
    }
}

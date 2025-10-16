using Bunit;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;
using NSubstitute;
using Xunit;

namespace BlazorApp.Tests;
public class AboutComponentTests : TestContext
{
    private AboutMe GetSampleAboutMe() => new AboutMe {
        Description = "Test description",
        Skills = ["C#", "Blazor"],
        DetailOrQuote = "Test quote",
        CurrentlyLearning =  ["Docker", "Azure"]
    };

    private HeroImage GetSampleHero() => new HeroImage {
        Src = "hero.jpg",
        Alt = "Hero Alt",
        Name = "about"
    };

    private HttpClient GetMockHttpClient(AboutMe? aboutMe)
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

    private IHeroImageService GetMockHeroImageService(HeroImage? hero)
    {
        var service = NSubstitute.Substitute.For<IHeroImageService>();
        service.GetHeroAsync(NSubstitute.Arg.Any<Func<HeroImage, bool>>()).Returns(hero);
        return service;
    }

    [Fact]
    public void RendersLoadingWhenAboutMeIsNull()
    {
        var httpClient = GetMockHttpClient(null);
        var heroService = GetMockHeroImageService(null);

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
        var heroService = GetMockHeroImageService(null);

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
        var heroService = GetMockHeroImageService(GetSampleHero());

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
        var heroService = GetMockHeroImageService(null);

        var cut = RenderComponent<About>(parameters => parameters
            .Add(p => p.Http, httpClient)
            .Add(p => p.HeroImageService, heroService)
        );

        cut.WaitForState(() => cut.Markup.Contains("Desc"));
        Assert.DoesNotContain("<li", cut.Markup); // No skills or learning items
    }

    // Helper stub for HttpClient mocking
    public class DelegatingHandlerStub : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;
        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handlerFunc(request, cancellationToken);
    }
}

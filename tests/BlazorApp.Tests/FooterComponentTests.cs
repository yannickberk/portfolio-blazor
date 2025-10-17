using System.Net;
using System.Net.Http.Json;
using Bunit;
using Xunit;
using BlazorApp.Shared;
using BlazorApp.Models;

namespace BlazorApp.Tests;

public class FooterComponentTests : TestContext
{
    private static SiteProperties SampleProperties => new()
    {
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

    private static HttpClient CreateMockHttpClient(
        SiteProperties? properties = null,
        SocialIcons? icons = null)
    {
        var handler = new DelegatingHandlerStub(async (request, cancellationToken) =>
        {
            if (request.RequestUri!.ToString().Contains("siteproperties.json"))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(properties)
                };
                return response;
            }
            if (request.RequestUri!.ToString().Contains("socialicons.json"))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(icons)
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
        var http = CreateMockHttpClient(null, null);

        var cut = RenderComponent<Footer>(parameters => parameters
            .Add(p => p.Http, http)
        );

        cut.MarkupMatches(@"
<div id=""contact"">
    <div class=""social-icons-container"">
        <p><em>Loading...</em></p>
    </div>
</div>");
    }

    [Fact]
    public async Task RendersAllSocialIconsAndFooterCredit()
    {
        var http = CreateMockHttpClient(SampleProperties, SampleIcons);

        var cut = RenderComponent<Footer>(parameters => parameters
            .Add(p => p.Http, http)
        );
        await cut.InvokeAsync(() => Task.CompletedTask);

        var icons = cut.FindAll("img.social-icon");
        Assert.Equal(8, icons.Count);

        Assert.Contains("Created by Jane Doe", cut.Markup);
    }

    [Fact]
    public async Task RendersOnlyNonEmptySocialLinks()
    {
        var props = new SiteProperties
        {
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
        var http = CreateMockHttpClient(props, SampleIcons);

        var cut = RenderComponent<Footer>(parameters => parameters
            .Add(p => p.Http, http)
        );
        await cut.InvokeAsync(() => Task.CompletedTask);

        var icons = cut.FindAll("img.social-icon");
        // Twitter and Medium should not be rendered
        Assert.DoesNotContain(icons, i => i.GetAttribute("alt") == "Dev.to" && i.GetAttribute("src") == SampleIcons.Twitter);
        Assert.DoesNotContain(icons, i => i.GetAttribute("alt") == "Dev.to" && i.GetAttribute("src") == SampleIcons.Medium);
        // Instagram should be rendered
        Assert.Contains(icons, i => i.GetAttribute("alt") == "Instagram");
    }

    [Fact]
    public async Task NoIconsIfIconsDataMissing()
    {
        var http = CreateMockHttpClient(SampleProperties, null);

        var cut = RenderComponent<Footer>(parameters => parameters
            .Add(p => p.Http, http)
        );
        await cut.InvokeAsync(() => Task.CompletedTask);

        Assert.Empty(cut.FindAll("img.social-icon"));
    }

    [Fact]
    public async Task FooterCreditNotRenderedIfPropertiesMissing()
    {
        var http = CreateMockHttpClient(null, SampleIcons);

        var cut = RenderComponent<Footer>(parameters => parameters
            .Add(p => p.Http, http)
        );
        await cut.InvokeAsync(() => Task.CompletedTask);

        Assert.DoesNotContain("footer-credit", cut.Markup);
    }
}
using BlazorApp.Models;
using BlazorApp.Services;
using NSubstitute;
using System;

namespace BlazorApp.Tests;
public static class TestHelpers
{
    public static IHeroImageService GetMockHeroImageService(HeroImage? hero)
    {
        var service = Substitute.For<IHeroImageService>();
        service.GetHeroAsync(Arg.Any<Func<HeroImage, bool>>()).Returns(hero);
        return service;
    }
}

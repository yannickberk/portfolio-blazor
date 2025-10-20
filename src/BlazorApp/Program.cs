using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp;
using BlazorApp.Services;

/// <summary>
/// Main entry point for the Blazor WebAssembly application.
/// Configures services, dependency injection, and starts the application.
/// </summary>
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure HTTP client
builder.Services.AddScoped(serviceProvider => 
{
    var httpClient = new HttpClient 
    { 
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
    };
    
    // Configure timeout for better performance
    httpClient.Timeout = TimeSpan.FromSeconds(30);
    
    return httpClient;
});

// Register application services
RegisterApplicationServices(builder.Services);

// Build and run the application
await builder.Build().RunAsync();

/// <summary>
/// Registers all application services for dependency injection.
/// </summary>
/// <param name="services">The service collection to register services with.</param>
static void RegisterApplicationServices(IServiceCollection services)
{
    // Register data services
    services.AddScoped<IHeroImageService, HeroImageService>();
    services.AddScoped<ISitePropertiesService, SitePropertiesService>();
    services.AddScoped<IProjectService, ProjectService>();
    services.AddScoped<IAboutMeService, AboutMeService>();
    services.AddScoped<ISocialIconsService, SocialIconsService>();
}

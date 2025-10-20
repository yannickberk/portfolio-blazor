using BlazorApp.Models;

namespace BlazorApp.Services;

/// <summary>
/// Service interface for managing site properties and configuration data.
/// </summary>
public interface ISitePropertiesService
{
    /// <summary>
    /// Retrieves the site properties containing personal information and social media links.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the site properties, or null if loading fails.</returns>
    Task<SiteProperties?> GetSitePropertiesAsync();
}

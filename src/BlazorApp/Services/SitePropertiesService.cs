using System.Net.Http.Json;
using BlazorApp.Models;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services;

/// <summary>
/// Implementation of <see cref="ISitePropertiesService"/> that loads site properties from JSON data.
/// </summary>
/// <param name="httpClient">The HTTP client used for fetching site properties data.</param>
/// <param name="logger">The logger instance for recording service operations.</param>
public sealed class SitePropertiesService(HttpClient httpClient, ILogger<SitePropertiesService> logger) : ISitePropertiesService
{
    private readonly ILogger<SitePropertiesService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lazy<Task<SiteProperties?>> _sitePropertiesLazy = new(() => LoadSitePropertiesAsync(httpClient, logger));

    /// <inheritdoc />
    public async Task<SiteProperties?> GetSitePropertiesAsync()
    {
        try
        {
            return await _sitePropertiesLazy.Value.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving site properties");
            return null;
        }
    }

    /// <summary>
    /// Loads site properties from the JSON data source.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making the request.</param>
    /// <param name="logger">The logger for recording load operations.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private static async Task<SiteProperties?> LoadSitePropertiesAsync(HttpClient httpClient, ILogger logger)
    {
        try
        {
            logger.LogDebug("Loading site properties from sample-data/siteproperties.json");
            
            var siteProperties = await httpClient.GetFromJsonAsync<SiteProperties>("sample-data/siteproperties.json")
                .ConfigureAwait(false);
            
            logger.LogInformation("Successfully loaded site properties");
            return siteProperties;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error occurred while loading site properties");
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error occurred while loading site properties");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while loading site properties");
            return null;
        }
    }
}
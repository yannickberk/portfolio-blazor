using System.Net.Http.Json;
using BlazorApp.Models;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services;

/// <summary>
/// Service interface for managing hero images in the portfolio application.
/// </summary>
public interface IHeroImageService
{
    /// <summary>
    /// Retrieves a hero image that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each hero image for a condition.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first hero image that matches the predicate, or null if no match is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the predicate is null.</exception>
    Task<HeroImage?> GetHeroAsync(Func<HeroImage, bool> predicate);

    /// <summary>
    /// Retrieves all available hero images.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of all hero images.</returns>
    Task<IReadOnlyList<HeroImage>> GetAllHeroImagesAsync();
}

/// <summary>
/// Implementation of <see cref="IHeroImageService"/> that loads hero images from JSON data.
/// </summary>
/// <param name="httpClient">The HTTP client used for fetching hero image data.</param>
/// <param name="logger">The logger instance for recording service operations.</param>
public sealed class HeroImageService(HttpClient httpClient, ILogger<HeroImageService> logger) : IHeroImageService
{
    private readonly ILogger<HeroImageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lazy<Task<IReadOnlyList<HeroImage>>> _heroImagesLazy = new(() => LoadHeroImagesAsync(httpClient, logger));

    /// <inheritdoc />
    public async Task<HeroImage?> GetHeroAsync(Func<HeroImage, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        try
        {
            var heroImages = await _heroImagesLazy.Value.ConfigureAwait(false);
            var result = heroImages.FirstOrDefault(predicate);
            
            _logger.LogDebug("Hero image search completed. Found: {Found}", result is not null);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for hero image");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HeroImage>> GetAllHeroImagesAsync()
    {
        try
        {
            return await _heroImagesLazy.Value.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all hero images");
            return [];
        }
    }

    /// <summary>
    /// Loads hero images from the JSON data source.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making the request.</param>
    /// <param name="logger">The logger for recording load operations.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private static async Task<IReadOnlyList<HeroImage>> LoadHeroImagesAsync(HttpClient httpClient, ILogger logger)
    {
        try
        {
            logger.LogDebug("Loading hero images from sample-data/heroimages.json");
            
            var heroImages = await httpClient.GetFromJsonAsync<List<HeroImage>>("sample-data/heroimages.json")
                .ConfigureAwait(false);
            
            var result = (heroImages ?? []).AsReadOnly();
            logger.LogInformation("Successfully loaded {Count} hero images", result.Count);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error occurred while loading hero images");
            return [];
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error occurred while loading hero images");
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while loading hero images");
            return [];
        }
    }
}
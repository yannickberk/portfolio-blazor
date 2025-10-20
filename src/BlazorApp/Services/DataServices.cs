using System.Net.Http.Json;
using BlazorApp.Models;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Services;

/// <summary>
/// Implementation of <see cref="IProjectService"/> that loads projects from JSON data.
/// </summary>
/// <param name="httpClient">The HTTP client used for fetching projects data.</param>
/// <param name="logger">The logger instance for recording service operations.</param>
public sealed class ProjectService(HttpClient httpClient, ILogger<ProjectService> logger) : IProjectService
{
    private readonly ILogger<ProjectService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lazy<Task<IReadOnlyList<Project>>> _projectsLazy = new(() => LoadProjectsAsync(httpClient, logger));

    /// <inheritdoc />
    public async Task<IReadOnlyList<Project>> GetProjectsAsync()
    {
        try
        {
            return await _projectsLazy.Value.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving projects");
            return [];
        }
    }

    /// <summary>
    /// Loads projects from the JSON data source.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making the request.</param>
    /// <param name="logger">The logger for recording load operations.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private static async Task<IReadOnlyList<Project>> LoadProjectsAsync(HttpClient httpClient, ILogger logger)
    {
        try
        {
            logger.LogDebug("Loading projects from sample-data/projects.json");
            
            var projects = await httpClient.GetFromJsonAsync<List<Project>>("sample-data/projects.json")
                .ConfigureAwait(false);
            
            var result = (projects ?? []).AsReadOnly();
            logger.LogInformation("Successfully loaded {Count} projects", result.Count);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error occurred while loading projects");
            return [];
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error occurred while loading projects");
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while loading projects");
            return [];
        }
    }
}

/// <summary>
/// Implementation of <see cref="IAboutMeService"/> that loads about me information from JSON data.
/// </summary>
/// <param name="httpClient">The HTTP client used for fetching about me data.</param>
/// <param name="logger">The logger instance for recording service operations.</param>
public sealed class AboutMeService(HttpClient httpClient, ILogger<AboutMeService> logger) : IAboutMeService
{
    private readonly ILogger<AboutMeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lazy<Task<AboutMe?>> _aboutMeLazy = new(() => LoadAboutMeAsync(httpClient, logger));

    /// <inheritdoc />
    public async Task<AboutMe?> GetAboutMeAsync()
    {
        try
        {
            return await _aboutMeLazy.Value.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving about me information");
            return null;
        }
    }

    /// <summary>
    /// Loads about me information from the JSON data source.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making the request.</param>
    /// <param name="logger">The logger for recording load operations.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private static async Task<AboutMe?> LoadAboutMeAsync(HttpClient httpClient, ILogger logger)
    {
        try
        {
            logger.LogDebug("Loading about me information from sample-data/aboutme.json");
            
            var aboutMe = await httpClient.GetFromJsonAsync<AboutMe>("sample-data/aboutme.json")
                .ConfigureAwait(false);
            
            logger.LogInformation("Successfully loaded about me information");
            return aboutMe;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error occurred while loading about me information");
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error occurred while loading about me information");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while loading about me information");
            return null;
        }
    }
}

/// <summary>
/// Implementation of <see cref="ISocialIconsService"/> that loads social icons from JSON data.
/// </summary>
/// <param name="httpClient">The HTTP client used for fetching social icons data.</param>
/// <param name="logger">The logger instance for recording service operations.</param>
public sealed class SocialIconsService(HttpClient httpClient, ILogger<SocialIconsService> logger) : ISocialIconsService
{
    private readonly ILogger<SocialIconsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Lazy<Task<SocialIcons?>> _socialIconsLazy = new(() => LoadSocialIconsAsync(httpClient, logger));

    /// <inheritdoc />
    public async Task<SocialIcons?> GetSocialIconsAsync()
    {
        try
        {
            return await _socialIconsLazy.Value.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving social icons");
            return null;
        }
    }

    /// <summary>
    /// Loads social icons from the JSON data source.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making the request.</param>
    /// <param name="logger">The logger for recording load operations.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private static async Task<SocialIcons?> LoadSocialIconsAsync(HttpClient httpClient, ILogger logger)
    {
        try
        {
            logger.LogDebug("Loading social icons from sample-data/socialicons.json");

            var socialIcons = await httpClient.GetFromJsonAsync<SocialIcons>("sample-data/socialicons.json")
                .ConfigureAwait(false);

            logger.LogInformation("Successfully loaded social icons");
            return socialIcons;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error occurred while loading social icons");
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error occurred while loading social icons");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while loading social icons");
            return null;
        }
    }
}

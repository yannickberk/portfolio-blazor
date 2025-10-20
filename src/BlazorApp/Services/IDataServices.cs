using BlazorApp.Models;

namespace BlazorApp.Services;

/// <summary>
/// Service interface for managing portfolio projects data.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Retrieves all portfolio projects.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of all projects.</returns>
    Task<IReadOnlyList<Project>> GetProjectsAsync();
}

/// <summary>
/// Service interface for managing personal information and about section data.
/// </summary>
public interface IAboutMeService
{
    /// <summary>
    /// Retrieves the about me information including description, skills, and learning details.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the about me information, or null if loading fails.</returns>
    Task<AboutMe?> GetAboutMeAsync();
}

/// <summary>
/// Service interface for managing social media icons.
/// </summary>
public interface ISocialIconsService
{
    /// <summary>
    /// Retrieves the social media icon URLs.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the social icons information, or null if loading fails.</returns>
    Task<SocialIcons?> GetSocialIconsAsync();
}

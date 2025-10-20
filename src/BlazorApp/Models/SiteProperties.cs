namespace BlazorApp.Models;

/// <summary>
/// Represents the main site properties and social media links for the portfolio.
/// </summary>
public sealed record SiteProperties
{
    /// <summary>
    /// Gets or sets the name of the site owner.
    /// </summary>
    /// <value>The full name displayed on the portfolio site.</value>
    public required string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the professional title or role.
    /// </summary>
    /// <value>The job title or professional description displayed under the name.</value>
    public required string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact email address.
    /// </summary>
    /// <value>The primary email address for professional contact.</value>
    public required string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Dev.to profile URL.
    /// </summary>
    /// <value>The URL to the Dev.to profile page.</value>
    public string DevDotTo { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub profile URL.
    /// </summary>
    /// <value>The URL to the GitHub profile page.</value>
    public string GitHub { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Instagram profile URL.
    /// </summary>
    /// <value>The URL to the Instagram profile page.</value>
    public string Instagram { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the LinkedIn profile URL.
    /// </summary>
    /// <value>The URL to the LinkedIn profile page.</value>
    public string LinkedIn { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Medium profile URL.
    /// </summary>
    /// <value>The URL to the Medium profile page.</value>
    public string Medium { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter profile URL.
    /// </summary>
    /// <value>The URL to the Twitter profile page.</value>
    public string Twitter { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the YouTube channel URL.
    /// </summary>
    /// <value>The URL to the YouTube channel page.</value>
    public string YouTube { get; init; } = string.Empty;
}
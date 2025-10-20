namespace BlazorApp.Models;

/// <summary>
/// Represents the social media icon URLs for the portfolio footer.
/// </summary>
public sealed record SocialIcons
{
    /// <summary>
    /// Gets or sets the email icon URL.
    /// </summary>
    /// <value>The path to the email icon image file.</value>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Dev.to icon URL.
    /// </summary>
    /// <value>The path to the Dev.to icon image file.</value>
    public string DevDotTo { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub icon URL.
    /// </summary>
    /// <value>The path to the GitHub icon image file.</value>
    public string GitHub { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Instagram icon URL.
    /// </summary>
    /// <value>The path to the Instagram icon image file.</value>
    public string Instagram { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the LinkedIn icon URL.
    /// </summary>
    /// <value>The path to the LinkedIn icon image file.</value>
    public string LinkedIn { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Medium icon URL.
    /// </summary>
    /// <value>The path to the Medium icon image file.</value>
    public string Medium { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter icon URL.
    /// </summary>
    /// <value>The path to the Twitter icon image file.</value>
    public string Twitter { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the YouTube icon URL.
    /// </summary>
    /// <value>The path to the YouTube icon image file.</value>
    public string YouTube { get; init; } = string.Empty;
}
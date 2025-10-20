namespace BlazorApp.Models;

/// <summary>
/// Represents a portfolio project with its details and metadata.
/// </summary>
public sealed record Project
{
    /// <summary>
    /// Gets or sets the title of the project.
    /// </summary>
    /// <value>The display name of the project shown in the portfolio.</value>
    public required string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the project.
    /// </summary>
    /// <value>A comprehensive description explaining what the project does and its key features.</value>
    public required string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the project repository or live demo.
    /// </summary>
    /// <value>The web address where users can view the project source code or live application.</value>
    public required string Url { get; init; } = string.Empty;
}
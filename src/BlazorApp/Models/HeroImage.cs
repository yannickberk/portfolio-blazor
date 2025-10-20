namespace BlazorApp.Models;

/// <summary>
/// Represents a hero image used in the portfolio application.
/// </summary>
public sealed record HeroImage
{
    /// <summary>
    /// Gets or sets the unique name identifier for the hero image.
    /// </summary>
    /// <value>The name of the hero image, used for identification and filtering.</value>
    public required string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the source URL or path to the image file.
    /// </summary>
    /// <value>The relative or absolute path to the image resource.</value>
    public required string Src { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the alternative text for accessibility purposes.
    /// </summary>
    /// <value>Descriptive text used by screen readers and displayed when image fails to load.</value>
    public required string Alt { get; init; } = string.Empty;
}
namespace BlazorApp.Models;

/// <summary>
/// Represents personal information and professional details for the About section.
/// </summary>
public sealed record AboutMe
{
    /// <summary>
    /// Gets or sets the personal description or biography.
    /// </summary>
    /// <value>A detailed description of the person's background, experience, and interests.</value>
    public required string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of professional skills and technologies.
    /// </summary>
    /// <value>A collection of skills, programming languages, frameworks, and tools the person is proficient in.</value>
    public IReadOnlyList<string> Skills { get; init; } = [];

    /// <summary>
    /// Gets or sets the list of technologies or skills currently being learned.
    /// </summary>
    /// <value>A collection of new technologies, frameworks, or skills the person is actively studying.</value>
    public IReadOnlyList<string> CurrentlyLearning { get; init; } = [];

    /// <summary>
    /// Gets or sets an additional detail or inspirational quote.
    /// </summary>
    /// <value>A personal quote, motto, or additional information that provides insight into the person's character or philosophy.</value>
    public string DetailOrQuote { get; init; } = string.Empty;
}
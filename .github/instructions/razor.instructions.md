---
description: 'Guidelines for building Razor Components'
applyTo: '**/*.razor'
---

# Razor Component Development Instructions

## Goals
- **Testability:** Components are easy to unit and integration test (Bunit/xUnit).
- **Maintainability:** Code is modular, readable, and follows SOLID principles.
- **Security:** Avoids common vulnerabilities (XSS, injection, data leaks).
- **Performance:** Efficient rendering, minimal allocations, and optimal data flow.

---

## 1. Component Structure & Organization

- Place components by feature in `Components/` or `Shared/`.
- Use PascalCase for component names.
- Keep each component focused on a single responsibility.
- Extract reusable logic into child components or services.

## 2. Parameters & Data Flow

- Use `[Parameter, EditorRequired]` for all required parameters.
- Prefer one-way data flow: parent passes data down, child raises events up.
- Use `EventCallback<T>` for event handling.
- Use two-way binding (`@bind`) only for form inputs requiring immediate synchronization, such as text inputs, sliders, or checkboxes.
  - **Example:** For a text input that updates a value as the user types:
    ```razor
    <InputText @bind-Value="searchTerm" />
    ```
  - For other scenarios, prefer explicit one-way data flow with `Value` and `ValueChanged` parameters and `EventCallback<T>` for updates.

## 3. Dependency Injection & Services

- Inject services (e.g., `HttpClient`, data services) via `[Inject]` or constructor injection.
- Encapsulate data access in services that implement interfaces for testability.
- Avoid direct file or network access in components.

## 4. Asynchronous Data Loading

- Load data asynchronously in `OnInitializedAsync` or `OnParametersSetAsync`.
- Use `Task`-returning methods and `await` for async operations.
- Show loading indicators or skeletons while awaiting data.

## 5. State Management

- Minimize component state; prefer parameters and cascading values.
- Use `StateHasChanged()` only when necessary.
- Avoid unnecessary re-renders by splitting large components.

## 6. Styling

- Use SCSS modules in `wwwroot/scss/`, compiled to CSS, and scope styles to components with unique class names as needed.
- Avoid inline styles and `<style>` tags in Razor files.

## 7. Security Best Practices

- Never render untrusted HTML with `@((MarkupString)...)` unless sanitized.
- Validate and encode all user input/output.
- Avoid exposing sensitive data in parameters or markup.
- Use `[Authorize]` and role checks for protected UI.

## 8. Performance Optimization

- Use `@key` directive in `@foreach` for stable DOM diffing.
- Prefer `RenderFragment` and child content for composition.
- Avoid blocking calls and synchronous code in rendering lifecycle.
- Use `ShouldRender()` override to skip unnecessary renders.

## 9. Testing

- Write Bunit tests for all components:
    - Render with required parameters.
    - Assert markup, state, and event callbacks.
    - Mock services using interfaces and NSubstitute.
- Write xUnit tests for service logic.
- Use test data from `wwwroot/sample-data/`.

## 10. Documentation & Comments

- Add XML doc comments to public parameters and events.
- Document expected parameters, events, and usage in a README or summary.

---

## Example: Highly Testable Razor Component

Here's a comprehensive example that demonstrates all the best practices above:

### Component Implementation (`Components/UserProfile.razor`)

```razor
@using BlazorApp.Models
@using BlazorApp.Services
@inject IUserService UserService
@inject ILogger<UserProfile> Logger

<div class="user-profile" data-testid="user-profile">
    @if (IsLoading)
    {
        <div class="loading-skeleton" data-testid="loading-state">
            <em>Loading user profile...</em>
        </div>
    }
    else if (ErrorMessage is not null)
    {
        <div class="error-message" data-testid="error-state">
            <p>Error: @ErrorMessage</p>
            <button @onclick="RetryLoad" data-testid="retry-button">Retry</button>
        </div>
    }
    else if (User is not null)
    {
        <div class="user-content" data-testid="user-content">
            @if (IsValidAvatarUrl(User.AvatarUrl))
            {
                <img src="@User.AvatarUrl" alt="@($"{User.Name} avatar")" class="avatar" />
            }
            <h2>@User.Name</h2>
            <p class="bio">@User.Bio</p>
            
            @if (ShowContactInfo && User.Email is not null)
            {
                <div class="contact-info" data-testid="contact-info">
                    <a href="mailto:@User.Email">@User.Email</a>
                </div>
            }
            
            <button @onclick="HandleProfileClick" 
                    disabled="@IsActionDisabled" 
                    data-testid="profile-action-button">
                @ActionButtonText
            </button>
        </div>
    }
</div>

@code {
    private User? User;
    private bool IsLoading = true;
    private string? ErrorMessage;

    /// <summary>
    /// Validates that the avatar URL is an absolute URI with http/https scheme.
    /// </summary>
    private static bool IsValidAvatarUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// The unique identifier of the user whose profile to display.
    /// </summary>
    [Parameter, EditorRequired] 
    public int UserId { get; set; }

    /// <summary>
    /// Whether to show contact information in the profile.
    /// </summary>
    [Parameter] 
    public bool ShowContactInfo { get; set; }

    /// <summary>
    /// Text to display on the action button.
    /// </summary>
    [Parameter] 
    public string ActionButtonText { get; set; } = "View Profile";

    /// <summary>
    /// Whether the action button is disabled.
    /// </summary>
    [Parameter] 
    public bool IsActionDisabled { get; set; }

    /// <summary>
    /// Event callback fired when the profile action button is clicked.
    /// </summary>
    [Parameter] 
    public EventCallback<User> OnProfileAction { get; set; }

    /// <summary>
    /// Event callback fired when a loading error occurs.
    /// </summary>
    [Parameter] 
    public EventCallback<string> OnError { get; set; }

    /// <summary>
    /// Initializes the component by loading user data.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadUserData();
    }

    /// <summary>
    /// Reloads component when UserId parameter changes.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (UserId > 0 && (User?.Id != UserId || ErrorMessage is not null))
        {
            await LoadUserData();
        }
    }

    /// <summary>
    /// Loads user data from the service with error handling.
    /// </summary>
    private async Task LoadUserData()
    {
        if (UserId <= 0)
        {
            Logger.LogWarning("Invalid UserId provided: {UserId}", UserId);
            ErrorMessage = "Invalid user ID";
            IsLoading = false;
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            StateHasChanged();

            Logger.LogDebug("Loading user data for UserId: {UserId}", UserId);
            User = await UserService.GetUserByIdAsync(UserId);

            if (User is null)
            {
                ErrorMessage = "User not found";
                await OnError.InvokeAsync("User not found");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load user data for UserId: {UserId}", UserId);
            ErrorMessage = "Failed to load user data";
            await OnError.InvokeAsync(ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles retry button click to reload user data.
    /// </summary>
    private async Task RetryLoad()
    {
        await LoadUserData();
    }

    /// <summary>
    /// Handles profile action button click.
    /// </summary>
    private async Task HandleProfileClick()
    {
        if (User is not null && !IsActionDisabled)
        {
            await OnProfileAction.InvokeAsync(User);
        }
    }
}
```

### Service Interface (`Services/IUserService.cs`)

```csharp
public interface IUserService
{
    Task<User?> GetUserByIdAsync(int userId);
}
```

### Model (`Models/User.cs`)

```csharp
public record User(
    int Id,
    string Name,
    string Bio,
    string AvatarUrl,
    string? Email
);
```

### Comprehensive Unit Tests (`Tests/UserProfileTests.cs`)

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;

public class UserProfileTests : TestContext
{
    private readonly IUserService _mockUserService;

    public UserProfileTests()
    {
        _mockUserService = Substitute.For<IUserService>();
        Services.AddSingleton(_mockUserService);
    }

    private static User GetSampleUser() => new(
        Id: 1,
        Name: "John Doe",
        Bio: "Software Developer",
        AvatarUrl: "https://example.com/avatar.jpg",
        Email: "john@example.com"
    );

    [Fact]
    [Trait("Category", "Loading")]
    public void Render_WhenLoading_ShowsLoadingState()
    {
        // Arrange
        var user = GetSampleUser();
        _mockUserService.GetUserByIdAsync(1).Returns(Task.FromResult<User?>(user));

        // Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1));

        // Assert - Check immediate loading state
        var loadingElement = cut.Find("[data-testid='loading-state']");
        Assert.NotNull(loadingElement);
        Assert.Contains("Loading user profile...", loadingElement.TextContent);

        // Assert - Wait for loading to finish and user content to appear
        cut.WaitForState(() => cut.FindAll("[data-testid='user-content']").Count > 0);
        var userContent = cut.Find("[data-testid='user-content']");
        Assert.Contains("John Doe", userContent.TextContent);

        // Assert - Loading state is no longer present
        Assert.Empty(cut.FindAll("[data-testid='loading-state']"));
    }

    [Fact]
    [Trait("Category", "Data")]
    public void Render_WithValidUser_DisplaysUserInformation()
    {
        // Arrange
        var user = GetSampleUser();
        _mockUserService.GetUserByIdAsync(1).Returns(user);

        // Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1));

        // Assert - Wait for async loading to complete
        cut.WaitForState(() => cut.FindAll("[data-testid='user-content']").Count > 0);
        
        var userContent = cut.Find("[data-testid='user-content']");
        Assert.Contains("John Doe", userContent.TextContent);
        Assert.Contains("Software Developer", userContent.TextContent);
        
        var avatar = userContent.QuerySelector("img");
        Assert.Equal("https://example.com/avatar.jpg", avatar?.GetAttribute("src"));
        Assert.Equal("John Doe avatar", avatar?.GetAttribute("alt"));
    }

    [Fact]
    [Trait("Category", "Parameters")]
    public void Render_WithShowContactInfoTrue_DisplaysContactInformation()
    {
        // Arrange
        var user = GetSampleUser();
        _mockUserService.GetUserByIdAsync(1).Returns(user);

        // Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1)
            .Add(p => p.ShowContactInfo, true));

        // Assert
        cut.WaitForState(() => cut.FindAll("[data-testid='contact-info']").Count > 0);
        
        var contactInfo = cut.Find("[data-testid='contact-info']");
        Assert.Contains("john@example.com", contactInfo.TextContent);
    }

    [Fact]
    [Trait("Category", "Events")]
    public async Task ProfileActionButton_WhenClicked_InvokesCallback()
    {
        // Arrange
        var user = GetSampleUser();
        var profileActionInvoked = false;
        User? callbackUser = null;

        _mockUserService.GetUserByIdAsync(1).Returns(user);

        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1)
            .Add(p => p.OnProfileAction, EventCallback.Factory.Create<User>(this, (u) => {
                profileActionInvoked = true;
                callbackUser = u;
            }));

        // Wait for component to load
        cut.WaitForState(() => cut.FindAll("[data-testid='profile-action-button']").Count > 0);

        // Assert - Button should be enabled before clicking
        var actionButton = cut.Find("[data-testid='profile-action-button']");
        Assert.False(actionButton.HasAttribute("disabled"));

        // Act
        await actionButton.ClickAsync();

        // Assert
        Assert.True(profileActionInvoked);
        Assert.Equal(user, callbackUser);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task Render_WhenServiceThrows_ShowsErrorState()
    {
        // Arrange
        _mockUserService.GetUserByIdAsync(1).ThrowsAsync(new Exception("Service error"));

        // Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1));

        // Assert
        cut.WaitForState(() => cut.FindAll("[data-testid='error-state']").Count > 0);
        
        var errorElement = cut.Find("[data-testid='error-state']");
        Assert.Contains("Failed to load user data", errorElement.TextContent);
        
        var retryButton = cut.Find("[data-testid='retry-button']");
        Assert.NotNull(retryButton);
    }

    [Fact]
    [Trait("Category", "Error")]
    public void Render_WhenServiceReturnsNull_ShowsUserNotFoundError()
    {
        // Arrange
        _mockUserService.GetUserByIdAsync(1).Returns((User?)null);

        // Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1));

        // Assert
        cut.WaitForState(() => cut.FindAll("[data-testid='error-state']").Count > 0);

        var errorElement = cut.Find("[data-testid='error-state']");
        Assert.Contains("User not found", errorElement.TextContent);

        var retryButton = cut.Find("[data-testid='retry-button']");
        Assert.NotNull(retryButton);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task Render_WhenServiceThrows_ShowsErrorState_AndInvokesOnError()
    {
        // Arrange
        _mockUserService.GetUserByIdAsync(1).ThrowsAsync(new Exception("Service error"));
        string? errorCallbackMessage = null;

        // Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1)
            .Add(p => p.OnError, EventCallback.Factory.Create<string>(this, (msg) => errorCallbackMessage = msg)));

        // Assert
        cut.WaitForState(() => cut.FindAll("[data-testid='error-state']").Count > 0);
        
        var errorElement = cut.Find("[data-testid='error-state']");
        Assert.Contains("Failed to load user data", errorElement.TextContent);
        
        var retryButton = cut.Find("[data-testid='retry-button']");
        Assert.NotNull(retryButton);

        // Assert OnError callback was invoked with the exception message
        Assert.Equal("Service error", errorCallbackMessage);
    }


    [Fact]
    [Trait("Category", "Validation")]
    public void Render_WithInvalidUserId_ShowsErrorState()
    {
        // Arrange & Act
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 0));

        // Assert
        cut.WaitForState(() => cut.FindAll("[data-testid='error-state']").Count > 0);
        var errorElement = cut.Find("[data-testid='error-state']");
        Assert.Contains("Invalid user ID", errorElement.TextContent);
    }

    [Fact]
    [Trait("Category", "Error")]
    public async Task RetryButton_WhenClicked_ReloadsData()
    {
        // Arrange
        _mockUserService.GetUserByIdAsync(1).ThrowsAsync(new Exception("First call fails"));
        
        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1));
        
        cut.WaitForState(() => cut.FindAll("[data-testid='retry-button']").Count > 0);

        // Setup success for retry
        var user = GetSampleUser();
        _mockUserService.GetUserByIdAsync(1).Returns(user);

        // Act
        var retryButton = cut.Find("[data-testid='retry-button']");
        await retryButton.ClickAsync();

        // Assert
        cut.WaitForState(() => cut.FindAll("[data-testid='user-content']").Count > 0);
        
        var userContent = cut.Find("[data-testid='user-content']");
        Assert.Contains("John Doe", userContent.TextContent);
    }

    [Fact]
    [Trait("Category", "Parameters")]
    public async Task OnParametersSet_WhenUserIdChanges_ReloadsData()
    {
        // Arrange
        var user1 = new User(1, "User One", "Bio 1", "avatar1.jpg", "user1@test.com");
        var user2 = new User(2, "User Two", "Bio 2", "avatar2.jpg", "user2@test.com");
        
        _mockUserService.GetUserByIdAsync(1).Returns(user1);
        _mockUserService.GetUserByIdAsync(2).Returns(user2);

        var cut = RenderComponent<UserProfile>(parameters => parameters
            .Add(p => p.UserId, 1));

        // Wait for first user to load
        cut.WaitForState(() => cut.Markup.Contains("User One"));

        // Act - Change UserId parameter
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.UserId, 2));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("User Two"));
        Assert.Contains("User Two", cut.Markup);
        Assert.DoesNotContain("User One", cut.Markup);
    }
}
```

### Key Testing Benefits Demonstrated:

1. **Service Mocking:** Uses NSubstitute to mock `IUserService`
2. **State Testing:** Validates loading, success, and error states
3. **Parameter Testing:** Tests component behavior with different parameter values
4. **Event Testing:** Verifies EventCallback invocation
5. **Async Testing:** Properly handles async component lifecycle
6. **Error Scenarios:** Tests exception handling and retry functionality
7. **Data Attributes:** Uses `data-testid` for reliable element selection
8. **Component Lifecycle:** Tests `OnParametersSetAsync` behavior

This example follows all the best practices outlined in the instructions and provides a comprehensive foundation for building maintainable, testable Blazor components.

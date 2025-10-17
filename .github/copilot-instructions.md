# Copilot Instructions for portfolio-blazor

## Project Overview
- This is a .NET Blazor WebAssembly portfolio site, structured as a single-page application (SPA).
- Main app code is in `src/BlazorApp/`. Components, models, services, and static assets are organized by feature.
- The entry point is `Pages/Index.razor`, which composes the UI from `Header`, `Home`, `About`, `Portfolio`, and `Footer` components.

## Architecture & Data Flow
- Data is loaded from JSON files in `wwwroot/sample-data/` using `HttpClient` and custom services (e.g., `HeroImageService`).
- Components receive dependencies via `[Parameter, EditorRequired]` or `@inject` for services and HTTP.
- Models (e.g., `SiteProperties`, `HeroImage`, `AboutMe`) define the shape of loaded data.
- Services encapsulate data fetching logic (see `Services/HeroImageService.cs`).
- SCSS is used for styling, compiled to CSS via npm scripts.

## Build & Development Workflow
- **Build:** Use `dotnet build src/BlazorApp/BlazorApp.csproj` or VS Code task `build`.
- **Run/Watch:** Use `dotnet watch run --project src/BlazorApp/BlazorApp.csproj` or VS Code task `watch` for hot-reload.
- **Publish:** Use `dotnet publish src/BlazorApp/BlazorApp.csproj` or VS Code task `publish`.
- **Sass Compilation:** SCSS in `wwwroot/scss/` is compiled to CSS in `wwwroot/css/` via `npm run sass:build` (auto-triggered before .NET build).
- **Tests:** Unit tests are in `tests/BlazorApp.Tests/`. Run with `dotnet test`.
  - Test frameworks: **xUnit** (unit tests), **Bunit** (Blazor component tests), **NSubstitute** (mocking).

## Conventions & Patterns
- All data-driven components load their data asynchronously from JSON files or services.
- Use `[Parameter, EditorRequired]` for required component parameters.
- Service classes should implement interfaces for testability and DI.
- Static assets (images, JSON) are served from `wwwroot/`.
- Use compressed CSS output for production (`sass:build`).

## Integration Points
- External dependencies: `Microsoft.AspNetCore.Components.WebAssembly`, `sass` (npm).
- No backend/API integration; all data is static or loaded from local files.
- Dev containers supported for Codespaces (see `.devcontainer/`).

## Examples
- To add a new section, create a Razor component in `Components/`, update `Index.razor`, and provide sample data in `wwwroot/sample-data/` if needed.
- To add a new model, define it in `Models/` and update relevant services/components.

---

For questions or improvements, see `README.md` or open an issue.

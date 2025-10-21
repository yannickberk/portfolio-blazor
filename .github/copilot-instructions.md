
# Copilot Instructions for portfolio-blazor

## Project Overview
- This is a Blazor WebAssembly SPA portfolio site. All data is loaded client-side from static JSON files in `wwwroot/sample-data/`.
- Main app code is in `src/BlazorApp/`. Components are organized by feature in `Components/` and `Shared/`.
- The entry point is `Pages/Index.razor`, which composes the UI from `Header`, `Home`, `About`, `Portfolio`, and `Footer` components.

## Architecture & Data Flow
- Data is loaded asynchronously via `HttpClient` (injected or passed as `[Parameter, EditorRequired]`).
- Models (e.g., `SiteProperties`, `HeroImage`, `Project`, `SocialIcons`) define the shape of loaded data.
- Services (e.g., `HeroImageService`) encapsulate data fetching logic and should implement interfaces for testability.
- Components expect required parameters and use `[Parameter, EditorRequired]` for enforcement.
- SCSS is used for styling, compiled to CSS via npm scripts. Source: `wwwroot/scss/`, output: `wwwroot/css/`.

## Build, Run, and Test Workflow
- **Build:** Use `dotnet build src/BlazorApp/BlazorApp.csproj` or VS Code task `build`.
- **Run/Watch:** Use `dotnet watch run --project src/BlazorApp/BlazorApp.csproj` or VS Code task `watch` for hot-reload.
- **Publish:** Use `dotnet publish src/BlazorApp/BlazorApp.csproj` or VS Code task `publish`.
- **Sass Compilation:** Run `npm run sass:build` to compile SCSS. This is auto-triggered before .NET build.
- **Tests:** Unit/component tests are in `tests/BlazorApp.Tests/`.
  - Use `dotnet test` to run all tests.
  - Test frameworks: **xUnit** (unit tests), **Bunit** (Blazor component tests), **NSubstitute** (mocking).

## Conventions & Patterns
- Use early returns whenever possible.
- All data-driven components load their data asynchronously from JSON files or services.
- Use `[Parameter, EditorRequired]` for required component parameters.
- Service classes implement interfaces for DI and testability.
- Static assets (images, JSON) are served from `wwwroot/`.
- Use compressed CSS output for production (`sass:build`).
- Tests use Bunit for rendering components and asserting markup/state. See `PortfolioComponentTests.cs`, `HeaderComponentTests.cs`, and `FooterComponentTests.cs` for examples.

## Code Style
- **Indentation:** 4 spaces for C#/Razor files, 2 spaces for JSON/YAML/XML files
- **Line endings:** CRLF (Windows-style)
- **Encoding:** UTF-8
- **Naming conventions:** Follow C# standard conventions (PascalCase for classes/methods, camelCase for local variables)
- See `.editorconfig` for complete style rules

## Integration Points
- External dependencies: `Microsoft.AspNetCore.Components.WebAssembly`, `sass` (npm), Bunit, NSubstitute.
- No backend/API integration; all data is static or loaded from local files.
- Dev containers supported for Codespaces (see `.devcontainer/`).

## Setup & Prerequisites
- **Required:** .NET 9.0 SDK
- **Required:** Node.js 18+ (for Sass compilation)
- **Optional:** Docker/Dev Containers for Codespaces
- Install npm dependencies before building: `cd src/BlazorApp && npm install`
- The project uses a pre-build target to compile SCSS automatically

## Examples & How-Tos
- To add a new section: create a Razor component in `Components/`, update `Pages/Index.razor`, and provide sample data in `wwwroot/sample-data/` if needed.
- To add a new model: define it in `Models/` and update relevant services/components.
- To add tests: use Bunit and xUnit in `tests/BlazorApp.Tests/`, mocking data/services as needed.

## CI/CD & Deployment
- **GitHub Pages:** Uses `.github/workflows/publish-gh-pages.yml` for automated deployment
- The workflow:
  1. Installs Node.js dependencies
  2. Compiles Sass to CSS
  3. Restores and builds .NET solution
  4. Runs tests
  5. Deploys to GitHub Pages
- Pull requests trigger builds and tests automatically

## Common Issues & Troubleshooting
- **Build error "npm not found":** Install Node.js 18+ and run `npm install` in `src/BlazorApp/`
- **SCSS compilation errors:** Ensure `sass` package is installed via `npm install`
- **Test failures:** Check that all sample data files in `wwwroot/sample-data/` are valid JSON
- **Missing dependencies:** Run `dotnet restore` from repository root

---

For questions or improvements, see `README.md` or open an issue.

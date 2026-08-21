---
name: 📦 Package Update
about: Track outdated or vulnerable packages
title: 'Dependencies: Update Microsoft.Extensions.Caching.Memory to stable version'
labels: 'dependencies, enhancement'
---

## Summary
The project currently has a dependency on a **release candidate (RC)** version of a NuGet package, which should be replaced with a stable version.

## Details

### 🚨 Critical Package Issue

**Package**: `Microsoft.Extensions.Caching.Memory`
- **Current Version**: `9.0.0-rc.2.24473.5` (Release Candidate)
- **Location**: `tests/BlazorApp.Tests/BlazorApp.Tests.csproj`
- **Severity**: High

### Why This Matters
- Release Candidate versions are **pre-release** and not recommended for production code
- RC versions can have breaking changes and stability issues
- May cause compatibility issues with other stable dependencies
- Maintenance support for RC versions is limited

## Action Items

- [ ] Update `Microsoft.Extensions.Caching.Memory` from `9.0.0-rc.2.24473.5` to stable version `9.0.0` or later
- [ ] Run all unit tests to ensure compatibility
- [ ] Verify no breaking changes are introduced
- [ ] Update package.lock files if applicable

## Affected Files
- `tests/BlazorApp.Tests/BlazorApp.Tests.csproj`

## Expected Outcome
All NuGet packages should use stable, production-ready versions aligned with the project's .NET 9.0 target framework.

---

**Package Status Report**:
✅ All other packages are on stable versions
- Microsoft.AspNetCore.Components.WebAssembly: 9.0.0
- Microsoft.NET.Test.Sdk: 17.12.0
- xunit: 2.9.2
- Bunit: 1.27.17
- NSubstitute: 5.0.0
- Shouldly: 4.2.1
- coverlet.collector: 6.0.2

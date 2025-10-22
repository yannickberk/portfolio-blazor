# GitHub Actions Workflows

This directory contains the CI/CD workflows for the portfolio Blazor application.

## Workflows

### 1. `publish-gh-pages.yml`

**Purpose**: Build, test, and deploy the Blazor app to GitHub Pages.

**Triggers**:
- Push to `main` branch
- Pull request events (opened, synchronize, reopened, closed)
- Manual workflow dispatch

**Jobs**:
- **build_test_artifact**: Builds and tests the app, creates deployment artifact
- **deploy**: Deploys to production GitHub Pages (main branch only)
- **pr-preview**: Deploys PR preview to temporary environment
- **cleanup-pr-preview**: Removes PR preview environment when PR is closed

**Requirements**:
- Node.js 18+ for Sass compilation
- .NET 9.0 SDK
- `ENV_CLEANUP_TOKEN` secret for environment deletion (see setup below)

### 2. `cleanup-orphaned-pr-previews.yml`

**Purpose**: Daily scheduled cleanup of orphaned PR preview environments.

**Triggers**:
- Daily at 2 AM UTC (scheduled)
- Manual workflow dispatch

**Jobs**:
- **identify-orphaned-environments**: Finds PR environments without matching open PRs
- **cleanup-orphaned-environments**: Deletes identified orphaned environments

**Requirements**:
- `ENV_CLEANUP_TOKEN` secret for environment deletion (see setup below)

### 3. `copilot-setup-steps.yml`

**Purpose**: Test workflow for Copilot setup validation.

**Triggers**:
- Manual workflow dispatch only

## Setup Required

### Environment Cleanup Token

Both cleanup workflows require a Personal Access Token (PAT) to delete environments, as the default `GITHUB_TOKEN` lacks this permission.

**⚠️ Without this token, environment cleanup will fail gracefully but environments will persist.**

**Setup Instructions**: See [../ENVIRONMENT_CLEANUP_SETUP.md](../ENVIRONMENT_CLEANUP_SETUP.md) for detailed instructions.

**Quick Setup**:
1. Create a fine-grained PAT with "Administration" repository permission (write)
2. Add it as a repository secret named `ENV_CLEANUP_TOKEN`
3. Verify by triggering a workflow run

## Monitoring

### View Workflow Runs

1. Go to the **Actions** tab in the repository
2. Select a workflow from the left sidebar
3. View run history and logs

### Check Environments

1. Go to **Settings** → **Environments**
2. View all active environments and their deployments
3. Manually delete orphaned environments if needed

## Troubleshooting

### Common Issues

**Issue**: Workflow fails with "Resource not accessible by integration"
- **Cause**: Missing or invalid `ENV_CLEANUP_TOKEN`
- **Solution**: Follow setup guide to create and configure the token

**Issue**: PR preview environment not deleted after merge
- **Cause**: Token not configured or workflow was canceled
- **Solution**: Configure token OR manually run cleanup workflow

**Issue**: Build fails with "npm not found"
- **Cause**: Node.js dependencies not installed
- **Solution**: Workflow handles this automatically; check logs for errors

### Get Help

- Check workflow logs for detailed error messages
- Review [../ENVIRONMENT_CLEANUP_SETUP.md](../ENVIRONMENT_CLEANUP_SETUP.md)
- Open an issue in the repository

## Customization

### Change Cleanup Schedule

Edit `cleanup-orphaned-pr-previews.yml`:

```yaml
schedule:
  - cron: '0 2 * * *'  # Daily at 2 AM UTC
```

Common alternatives:
- `'0 */6 * * *'` - Every 6 hours
- `'0 0 * * 0'` - Weekly on Sunday
- `'0 3 * * 1-5'` - Weekdays at 3 AM

### Disable Environment Cleanup

If you prefer manual environment management:

1. Comment out or remove the cleanup jobs in both workflows
2. Manage environments manually via Settings → Environments

## Security

- All workflows use `GITHUB_TOKEN` for most operations (read-only by default)
- Environment cleanup requires elevated permissions via `ENV_CLEANUP_TOKEN`
- Secrets are encrypted at rest and never exposed in logs
- Fine-grained tokens are recommended for better security

## References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Pages Deployment](https://docs.github.com/en/pages)
- [Environment Cleanup Setup](../ENVIRONMENT_CLEANUP_SETUP.md)
- [PR Preview Solution Details](../PR_PREVIEW_CLEANUP_SOLUTION.md)

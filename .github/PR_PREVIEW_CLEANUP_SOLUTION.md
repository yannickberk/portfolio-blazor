# PR Preview Environment Cleanup Solution

## Problem

When merging a PR, the workflow for cleaning up PR preview environments would be canceled before completion:

1. PR is merged/closed → workflow triggers from PR branch
2. Merge to main → workflow triggers from main branch
3. Both workflows use the same concurrency group (`pages`) with `cancel-in-progress: true`
4. Main branch workflow cancels the PR branch workflow
5. Cleanup job never runs → orphaned preview environments persist

## Solution

### 1. Dynamic Concurrency Groups

Modified `.github/workflows/publish-gh-pages.yml` to use different concurrency groups:

- **PR cleanup jobs**: Use `pages-cleanup-{PR_NUMBER}` as the concurrency group
- **Regular deployments**: Use `pages` as the concurrency group
- **Cancel-in-progress**: Disabled (`false`) for cleanup jobs, enabled (`true`) for others

This ensures cleanup jobs cannot be canceled by subsequent deployments.

### 2. Scheduled Cleanup Workflow

Created `.github/workflows/cleanup-orphaned-pr-previews.yml` as a safety net:

- Runs daily at 2 AM UTC (configurable via cron)
- Can be manually triggered via `workflow_dispatch`
- Compares all `pr-*` environments against open PRs
- Deletes environments for closed/merged PRs
- Logs all actions for debugging

## How It Works

### When a PR is Closed/Merged

1. Workflow triggers with `github.event.action == 'closed'`
2. Concurrency group is set to `pages-cleanup-{PR_NUMBER}` (unique per PR)
3. `cancel-in-progress` is set to `false`
4. Build job is skipped (conditional: `github.event.action != 'closed'`)
5. Cleanup job runs independently and completes without interruption
6. Preview environment is deleted successfully

### When a PR is Opened/Updated

1. Workflow triggers with `github.event.action` in `[opened, synchronize, reopened]`
2. Concurrency group is set to `pages`
3. `cancel-in-progress` is set to `true` (allows canceling old deployments)
4. Build job runs and creates artifact
5. PR preview job deploys to `pr-{branch-name}` environment

### When Pushing to Main

1. Workflow triggers with `github.event_name == 'push'`
2. Concurrency group is set to `pages`
3. `cancel-in-progress` is set to `true`
4. Build job runs and creates artifact
5. Deploy job deploys to production GitHub Pages
6. Does NOT cancel PR cleanup jobs (different concurrency group)

### Daily Cleanup (Scheduled)

1. Workflow runs daily at 2 AM UTC
2. Fetches all environments from the repository
3. Fetches all open PRs
4. Identifies orphaned `pr-*` environments (no matching open PR)
5. Deletes orphaned environments
6. Logs summary of cleaned environments

## Testing

To test this solution:

### Manual Testing

1. Create a test PR
2. Verify PR preview environment is created
3. Merge the PR
4. Check that the cleanup job completes (not canceled)
5. Verify the preview environment is deleted

### Simulating Orphaned Environments

1. Manually trigger the cleanup workflow: Actions → "Cleanup Orphaned PR Preview Environments" → Run workflow
2. Check logs to see if any orphaned environments were found
3. Verify cleanup was successful

### Monitoring

Check workflow runs in the Actions tab:
- `Publish Blazor App to GitHub Pages` - Check cleanup job completion
- `Cleanup Orphaned PR Preview Environments` - Check daily cleanup logs

## Acceptance Criteria

✅ Merging a PR will always clean up its preview environment
- Cleanup jobs use separate concurrency groups and won't be canceled

✅ No preview environments are left behind due to workflow cancellation
- Cleanup jobs have `cancel-in-progress: false` when closing PRs
- Scheduled workflow provides additional safety net

✅ Workflows should not compete or cancel essential cleanup operations
- PR cleanup uses `pages-cleanup-{PR_NUMBER}` group
- Regular deployments use `pages` group
- No overlap between groups

## Additional Benefits

1. **Resilience**: Even if the normal cleanup fails, the scheduled workflow will clean up orphaned environments
2. **Manual Control**: The scheduled workflow can be triggered manually if needed
3. **Logging**: Both workflows provide detailed logs for debugging
4. **Error Handling**: Graceful handling of 404 errors (environment already deleted)

## Configuration

### Adjusting Cleanup Schedule

Edit `.github/workflows/cleanup-orphaned-pr-previews.yml`:

```yaml
schedule:
  - cron: '0 2 * * *'  # Daily at 2 AM UTC
```

Common alternatives:
- `'0 */6 * * *'` - Every 6 hours
- `'0 0 * * 0'` - Weekly on Sunday at midnight
- `'0 3 * * 1-5'` - Weekdays at 3 AM

### Permissions

Both workflows require:
- `contents: write` (or `read` for cleanup-only)
- `deployments: write` (to delete environments)
- `pages: write` (for deployments)
- `id-token: write` (for GitHub Pages)

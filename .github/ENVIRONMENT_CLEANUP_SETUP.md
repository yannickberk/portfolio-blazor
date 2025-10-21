# Environment Cleanup Setup Guide

## Overview

This repository includes workflows to automatically clean up PR preview environments when PRs are closed or merged. However, due to GitHub Actions security limitations, **the default `GITHUB_TOKEN` cannot delete environments**. This guide explains how to set up proper authentication for environment cleanup.

## Problem

GitHub Actions workflows use the `GITHUB_TOKEN` for authentication, but this token has limited permissions. Specifically:

- ❌ The `GITHUB_TOKEN` **cannot** delete GitHub environments
- ❌ There is no `environments: write` permission available for `GITHUB_TOKEN`
- ✅ Deleting environments requires repository administration access

## Solution

To enable environment cleanup, you need to create a Personal Access Token (PAT) with the required permissions.

### Step 1: Create a Personal Access Token

1. Go to **GitHub Settings** → **Developer settings** → **Personal access tokens** → **Fine-grained tokens** (or use Classic tokens)
2. Click **Generate new token**
3. Configure the token:
   - **Name**: `Environment Cleanup Token` (or any descriptive name)
   - **Repository access**: Select "Only select repositories" and choose this repository
   - **Permissions** (for fine-grained tokens):
     - **Repository permissions**:
       - Administration: **Read and write** (required to delete environments)
       - Deployments: **Read and write**
       - Environments: **Read and write**
   - **Permissions** (for classic tokens):
     - Select **`repo`** (Full control of private repositories)
4. Click **Generate token** and **copy the token immediately** (you won't be able to see it again)

### Step 2: Add Token as Repository Secret

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Configure the secret:
   - **Name**: `ENV_CLEANUP_TOKEN`
   - **Value**: Paste the token you generated in Step 1
5. Click **Add secret**

### Step 3: Verify Configuration

Once the secret is added:

1. The workflows will automatically use the `ENV_CLEANUP_TOKEN` when available
2. If the secret is not configured, the workflows will fall back to `GITHUB_TOKEN` and log a warning
3. Test the setup by:
   - Creating a test PR
   - Merging or closing the PR
   - Checking the workflow run logs to confirm the environment was deleted

## Workflows That Require This Token

### 1. `cleanup-orphaned-pr-previews.yml`

- **Purpose**: Runs daily to clean up orphaned PR preview environments
- **Schedule**: 2 AM UTC daily (configurable)
- **Trigger**: Scheduled or manual via workflow dispatch
- **What it does**:
  1. Lists all environments with `pr-*` prefix
  2. Compares against currently open PRs
  3. Deletes environments for closed/merged PRs

### 2. `publish-gh-pages.yml`

- **Purpose**: Deploys the app and manages PR preview environments
- **Trigger**: PR events (open, sync, close) and pushes to main
- **Cleanup job**: Runs when a PR is closed to delete its preview environment

## Security Considerations

### Token Scope

The PAT requires broad permissions (`repo` scope or repository administration). Consider:

- ✅ Use **fine-grained tokens** for better security (recommended)
- ✅ Set an **expiration date** and renew periodically
- ✅ Limit access to **only this repository**
- ⚠️ Classic tokens with `repo` scope have full repository access

### Token Storage

- ✅ Store the token as a **repository secret** (encrypted at rest)
- ❌ **Never** commit the token to version control
- ❌ **Never** log or expose the token in workflow outputs

### Alternative: GitHub App

For organizations or teams, consider creating a GitHub App with repository administration permissions instead of using a PAT. This provides better security and audit capabilities.

## Troubleshooting

### "Resource not accessible by integration" Error

**Cause**: The `GITHUB_TOKEN` doesn't have permission to delete environments.

**Solution**: Follow the steps above to create and configure `ENV_CLEANUP_TOKEN`.

### Environment Not Deleted

**Possible causes**:

1. **Missing `ENV_CLEANUP_TOKEN` secret**: Check that the secret is configured correctly
2. **Token expired**: Fine-grained and classic tokens can expire - generate a new one
3. **Insufficient permissions**: Ensure the token has repository administration or `repo` scope
4. **Environment protected**: Check environment protection rules in repository settings

### Workflow Fails Silently

Both workflows use `continue-on-error: true` for the cleanup steps, so they won't fail the entire workflow if cleanup fails. Check the workflow logs for warnings or errors related to environment deletion.

## Testing

### Manual Test

1. Create a test PR (e.g., `test-environment-cleanup`)
2. Verify a `pr-test-environment-cleanup` environment is created
3. Close or merge the PR
4. Check workflow logs: **Actions** → **Publish Blazor App to GitHub Pages**
5. Verify the environment is deleted: **Settings** → **Environments**

### Scheduled Cleanup Test

1. Manually trigger the cleanup workflow: **Actions** → **Cleanup Orphaned PR Preview Environments** → **Run workflow**
2. Check the logs to see if any orphaned environments were found and deleted

## Disabling Environment Cleanup

If you don't want to set up a PAT and prefer to manually manage environments:

1. Remove or comment out the cleanup jobs in both workflow files
2. Manually delete environments via: **Settings** → **Environments** → Select environment → **Delete environment**

## References

- [GitHub REST API: Delete an environment](https://docs.github.com/en/rest/deployments/environments#delete-an-environment)
- [GitHub Actions: GITHUB_TOKEN permissions](https://docs.github.com/en/actions/security-guides/automatic-token-authentication#permissions-for-the-github_token)
- [Creating a fine-grained personal access token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token#creating-a-fine-grained-personal-access-token)
- [strumwolf/delete-deployment-environment action](https://github.com/strumwolf/delete-deployment-environment)

## Questions or Issues?

If you encounter problems or have questions about environment cleanup:

1. Check the workflow run logs for detailed error messages
2. Verify the token permissions and expiration
3. Consult the [GitHub Actions documentation](https://docs.github.com/en/actions)
4. Open an issue in this repository

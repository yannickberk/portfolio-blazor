# Workflow Behavior Comparison

## Before Fix (Problem)

```
PR Merge Event Sequence:
┌─────────────────────────────────────────────────────────────┐
│ 1. PR Merged                                                │
├─────────────────────────────────────────────────────────────┤
│ 2. Workflow triggers from PR branch                         │
│    - Concurrency group: "pages"                             │
│    - cancel-in-progress: true                               │
│    - cleanup-pr-preview job queued                          │
├─────────────────────────────────────────────────────────────┤
│ 3. Merge commits to main                                    │
│    - Workflow triggers from main branch                     │
│    - Concurrency group: "pages" (SAME!)                     │
│    - cancel-in-progress: true                               │
├─────────────────────────────────────────────────────────────┤
│ 4. Main workflow CANCELS PR workflow                        │
│    ❌ cleanup-pr-preview job never runs                     │
├─────────────────────────────────────────────────────────────┤
│ 5. Result: PR preview environment NOT deleted               │
│    ⚠️  Orphaned environment persists                        │
└─────────────────────────────────────────────────────────────┘
```

## After Fix (Solution)

```
PR Merge Event Sequence:
┌─────────────────────────────────────────────────────────────┐
│ 1. PR Merged                                                │
├─────────────────────────────────────────────────────────────┤
│ 2. Workflow triggers from PR branch                         │
│    - Concurrency group: "pages-cleanup-123"                 │
│    - cancel-in-progress: false                              │
│    - cleanup-pr-preview job starts                          │
├─────────────────────────────────────────────────────────────┤
│ 3. Merge commits to main                                    │
│    - Workflow triggers from main branch                     │
│    - Concurrency group: "pages" (DIFFERENT!)                │
│    - cancel-in-progress: true                               │
├─────────────────────────────────────────────────────────────┤
│ 4. Both workflows run simultaneously                        │
│    ✅ cleanup-pr-preview job completes                      │
│    ✅ main deployment proceeds                              │
├─────────────────────────────────────────────────────────────┤
│ 5. Result: PR preview environment deleted successfully      │
│    ✅ No orphaned environments                              │
└─────────────────────────────────────────────────────────────┘
```

## Concurrency Group Logic

### Decision Tree
```
Is this a PR close event?
│
├─ YES ──> Concurrency group: "pages-cleanup-{PR_NUMBER}"
│          cancel-in-progress: false
│          → Cleanup job runs to completion
│
└─ NO ───> Concurrency group: "pages"
           cancel-in-progress: true
           → Normal deployment behavior
```

## Scheduled Cleanup (Safety Net)

```
Daily at 2 AM UTC:
┌─────────────────────────────────────────────────────────────┐
│ 1. Fetch all environments                                   │
│    - Find all environments with "pr-" prefix                │
├─────────────────────────────────────────────────────────────┤
│ 2. Fetch all open PRs                                       │
│    - Get list of branch names from open PRs                 │
├─────────────────────────────────────────────────────────────┤
│ 3. Compare and identify orphans                             │
│    - pr-feature-x → No matching open PR → ORPHAN            │
│    - pr-feature-y → Has matching open PR → KEEP             │
├─────────────────────────────────────────────────────────────┤
│ 4. Delete orphaned environments                             │
│    ✅ Cleanup complete                                      │
└─────────────────────────────────────────────────────────────┘
```

## Key Improvements

1. **Separate Concurrency Groups**
   - Cleanup jobs: `pages-cleanup-{PR_NUMBER}` (unique per PR)
   - Deployments: `pages` (shared)
   - No more conflicts!

2. **Protected Cleanup Jobs**
   - `cancel-in-progress: false` for PR close events
   - Ensures cleanup always completes

3. **Automated Safety Net**
   - Daily scheduled cleanup
   - Catches any missed deletions
   - Manual trigger available

4. **Better Logging**
   - Both workflows log all actions
   - Easy to debug issues
   - Track what was cleaned up

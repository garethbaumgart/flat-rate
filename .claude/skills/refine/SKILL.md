---
name: refine
description: Refine and plan a GitHub issue for implementation. Reads the issue, explores the codebase, creates a UX mockup if needed, writes a step-by-step implementation plan, and updates the issue body.
---

# Refine Issue for Implementation

You are refining a GitHub issue into an actionable implementation plan. The argument is the issue number (e.g., `/refine 336`).

## Step 1: Read the Issue

```bash
gh issue view <number>
```

Read the full issue body. Extract:
- **Goal**: What needs to be built or changed
- **Acceptance criteria**: What "done" looks like
- **Design decision**: Any chosen mockup option or UX direction
- **Dependencies**: Other issues this depends on

## Step 2: Assess Clarity — Ask Clarifying Questions if Needed

Before exploring code, evaluate whether the issue is clear enough to plan:

**Ask clarifying questions if:**
- The issue has no acceptance criteria
- The scope is ambiguous ("improve the UI", "make it faster")
- There are multiple valid approaches and no design decision
- The issue references concepts not defined elsewhere

**Proceed without asking if:**
- The issue has clear acceptance criteria
- A design/mockup has already been chosen
- The scope and approach are unambiguous

Use `AskUserQuestion` to gather answers before continuing. Frame questions with specific options when possible.

## Step 3: Check for Existing Mockup

Look for a mockup that matches the issue:

```bash
ls mockups/ 2>/dev/null | grep -i <feature-keyword>
```

**If a mockup exists AND has a chosen design**: Read the mockup file, note the chosen option, and incorporate it into the plan.

**If a mockup exists but NO design is chosen**: Read it, then ask the user which option they prefer before proceeding.

**If NO mockup exists AND the issue involves UI changes**: Create one (see Step 4).

**If NO mockup exists AND the issue is backend-only**: Skip to Step 5.

## Step 4: Create UX Mockup (When Needed)

Create the `mockups/` directory if it doesn't exist (`mkdir -p mockups`), then create a standalone HTML mockup file at `mockups/<feature-name>.html` with **10 distinct design options**.

### Mockup Template

Follow the exact pattern used in existing mockups if any exist. Key requirements:

1. **Use the project's design system** — include Tailwind CDN, PrimeIcons, and any project-specific CSS variables for theming (light/dark mode).

2. **Include these standard elements:**
   - Tailwind CDN (`https://cdn.tailwindcss.com`)
   - PrimeIcons (`https://unpkg.com/primeicons@7.0.0/primeicons.css`)
   - Theme toggle button (fixed top-right)
   - `toggleTheme()` script

3. **For each option include:**
   - Numbered option card with title and subtitle
   - Visual mockup of the design in context
   - Pros/cons list (green for pros, red for cons)
   - Option dividers between each option

4. **Mark one option as "Recommended"** with:
   - Green border highlight
   - "Recommended" badge in the header
   - Clear reasoning for why it's recommended

5. **Include a comparison table** at the bottom summarizing all options across key dimensions.

6. **Include a "Recommendation" section** explaining the recommended choice.

After creating the mockup, open it for the user:
```bash
# macOS:
open mockups/<feature-name>.html
# Linux:
xdg-open mockups/<feature-name>.html
```

Then tell the user: "I've created a mockup with 10 design options and recommended Option X. Please review and let me know which option you prefer, or I'll proceed with the recommended one."

Wait for the user's response before continuing.

## Step 5: Explore the Codebase

Thoroughly explore all code relevant to the issue. **Be exhaustive** — missing a file here means a broken plan.

### What to Find

**Backend (always check all of these):**
- Relevant domain entities/aggregates (look in `src/FlatRate.Domain/Aggregates/`)
- Repository interfaces in Domain layer
- Repository implementations in Infrastructure layer (`src/FlatRate.Infrastructure/Persistence/Repositories/`)
- Application layer handlers/queries/commands (`src/FlatRate.Application/`)
- DTOs and response models
- API endpoints (`src/FlatRate.Web/Endpoints/`)
- Existing patterns to follow (e.g., how similar features were built)

**Frontend (always check all of these):**
- Models/interfaces (`*.model.ts`)
- Services (`*.service.ts`)
- Page components (`*.page.ts`)
- Shared components (`*.component.ts`)
- Template markup (inline templates in component files)

**Cross-cutting (always check):**
- Callers of methods being changed (use grep to find all call sites)
- Optimistic update patterns (look for `increment`/`decrement`/`update` calls on services)
- Existing tests for the area being modified (check `tests/` directory)
- Related mockups or design decisions

### Exploration Techniques

```bash
# Find relevant files by keyword
grep -r "MethodName" src/ --include="*.cs" --include="*.ts" -l

# Find all callers of a method
grep -r "methodName" src/ --include="*.ts" -l

# Check for existing tests
grep -r "ClassName" tests/ --include="*.cs" -l

# Understand the domain model
cat src/FlatRate.Domain/Aggregates/<Entity>/<Entity>.cs
```

**Read the full contents** of every relevant file. Don't just find file names — read the actual code to understand:
- Method signatures and return types
- How data flows from API → handler → repository → database
- What patterns exist that should be followed
- What optimistic update hooks exist on the frontend

## Step 6: Write the Implementation Plan

Write a concrete, step-by-step implementation plan with:

### Plan Structure

Each step must include:
1. **Step title** — what's being done
2. **Files table** — exact file paths and what changes in each
3. **Code snippets** — show the actual code to write (not pseudocode), referencing existing patterns

### Plan Rules

- **Order steps by dependency** — backend before frontend, interfaces before implementations
- **One concern per step** — don't mix repository changes with UI changes
- **Show the pattern to follow** — if there's existing code that does something similar, reference it explicitly (e.g., "Follow the same pattern as `BillRepository.GetByPropertyIdAsync` at line 28-42")
- **Include optimistic update changes** — if the change affects data that's optimistically updated on the frontend, include steps to update all callers
- **Include test expectations** — note what tests should be added or updated (but don't write the test code in the plan)

### Plan Template

```markdown
## Implementation Plan

### Step 1 — [Area]: [What]

**Files:**

| File | Change |
|------|--------|
| `path/to/file.cs` | Description of change |

**Pattern to follow** (from `path/to/existing.cs:28-42`):
\`\`\`csharp
// Existing code that shows the pattern
\`\`\`

**New code:**
\`\`\`csharp
// The actual code to write
\`\`\`

### Step 2 — ...
```

## Step 7: Update Labels

After writing the plan, update the issue labels:
- **Remove** the `to-refine` label
- **Add** the `refined` label
- Add any relevant feature labels

```bash
gh issue edit <number> --remove-label "to-refine" --add-label "refined"
```

## Step 8: Update the GitHub Issue Body

Replace the issue body with the refined version. **Preserve** the original sections (Overview, Dependencies, Chosen Design) and **add/overwrite** the Implementation Plan section.

Use `gh issue edit` with a heredoc:

```bash
gh issue edit <number> --body "$(cat <<'ISSUE_BODY'
## Overview
[preserved from original]

## Dependencies
[preserved from original]

## Chosen Design
[preserved or updated based on mockup selection]

---

## Implementation Plan

### Step 1 — ...
[the full plan]

---

## Acceptance Criteria
[preserved or refined from original]
ISSUE_BODY
)"
```

### What to Include in the Updated Issue

- **Overview** — keep the original, or refine for clarity
- **Dependencies** — preserve as-is
- **Chosen Design** — update if a mockup was created/selected in this session
- **Implementation Plan** — the full step-by-step plan from Step 6
- **Acceptance Criteria** — preserve original, add any new criteria discovered during planning

### What NOT to Include

- Internal exploration notes or reasoning
- Alternative approaches that were rejected
- Code that was read but not relevant to the plan

## Step 9: Summary

Tell the user:
1. What you found during exploration
2. How many steps the plan has
3. Any risks or decisions that need attention
4. Link to the updated issue

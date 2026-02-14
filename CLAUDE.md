# CLAUDE.md - Project Preferences

## Critical Rules (ENFORCED)

These rules cause the most common sub-agent mistakes. They are listed first for maximum visibility.

### Banned Patterns

```html
<!-- BANNED: Old structural directives -->
*ngIf, *ngFor, *ngSwitch, [ngClass]

<!-- USE INSTEAD: New control flow -->
@if (loading()) { <spinner /> }
@for (item of items(); track item.id) { ... }
@switch (status()) { @case ('done') { ... } }
[class.active]="isActive()"
```

### Zoneless Anti-Patterns

Angular 21 is zoneless by default. These patterns **will NOT work**:

```typescript
// WRONG: Plain properties don't trigger change detection
items: Item[] = [];
this.items.push(newItem);  // View won't update!

// CORRECT: Use signals instead
readonly items = signal<Item[]>([]);
this.items.update(arr => [...arr, newItem]);  // View updates!

// WRONG: Don't mutate signal values directly
this.items().push(newItem);  // Wrong!

// CORRECT: Always create new references
this.items.update(arr => [...arr, newItem]);  // Correct!
```

### Theming Rules

- **ALWAYS use semantic tokens** from the project's CSS (e.g., `bg-surface`, `text-foreground`)
- **NEVER use hardcoded Tailwind colors** (e.g., `bg-gray-100`, `text-violet-600`)
- **NEVER use `dark:` prefix** — CSS variable system handles dark mode automatically
- **Respect token semantics**: Use `-foreground` tokens for text, background tokens for backgrounds

### Boy Scout Rule (Scoped)

When modifying code, fix banned or discouraged patterns in the lines you're already changing — but do NOT expand scope.

**DO fix (within lines you're already editing):**
- `*ngIf` / `*ngFor` / `*ngSwitch` → `@if` / `@for` / `@switch` control flow
- `[ngClass]` → `[class.x]="expr"` bindings
- Hardcoded Tailwind colors (`bg-gray-100`) → semantic tokens (`bg-surface`)
- `dark:` prefixed classes → remove (CSS variable system handles it)
- Constructor injection → `inject()` DI

**DO NOT:**
- Refactor functions or methods you aren't otherwise changing
- Add types, comments, or docstrings to unchanged code
- Rename files or move code to different locations
- Create separate commits for Boy Scout cleanup — fold fixes into the feature commit
- Touch lines outside the scope of the current task

## Pattern Examples (Real Files)

When implementing a common pattern, use these real files as references instead of guessing.

### Backend Patterns

| Pattern | Exemplar File |
|---------|--------------|
| Domain aggregate with factory method | `src/FlatRate.Domain/Aggregates/Bills/Bill.cs` |
| Repository interface (Domain layer) | `src/FlatRate.Domain/Aggregates/Bills/IBillRepository.cs` |
| Repository implementation (Infrastructure) | `src/FlatRate.Infrastructure/Persistence/Repositories/BillRepository.cs` |
| CQRS Command handler | `src/FlatRate.Application/Features/Bills/CreateBill.cs` |
| CQRS Query handler | `src/FlatRate.Application/Features/Bills/GetBillsByProperty.cs` |
| DTO / Response model | `src/FlatRate.Application/Features/Bills/BillDto.cs` |
| Minimal API endpoints | `src/FlatRate.Web/Endpoints/BillEndpoints.cs` |
| Domain unit tests | `tests/FlatRate.Domain.Tests/` |

### Frontend Patterns

| Pattern | Exemplar File |
|---------|--------------|
| Feature service (signals, CRUD, API calls) | `src/FlatRate.Web/ClientApp/src/app/bills/bill.service.ts` |
| List page with loading/error/empty states | `src/FlatRate.Web/ClientApp/src/app/bills/bills.page.ts` |
| Feature model/interface | `src/FlatRate.Web/ClientApp/src/app/bills/bill.model.ts` |
| E2E test (Playwright) | `tests/FlatRate.E2E.Tests/tests/` |

## Backend (.NET 10)

- **Architecture**: Vertical Slice Architecture (feature folders, not layer folders)
- **Pattern**: CQRS (Command Query Responsibility Segregation)
- **DDD Principles**: Repository interfaces live in Domain layer
- **Clean Architecture Layers**: Domain → Application → Infrastructure → Web
- **Style**: Primary constructors, Minimal APIs
- **Database**: Entity Framework Core with PostgreSQL (all environments)

## Frontend (Angular v21)

### Core Principles

- **Standalone components only** - No NgModules
- **Signals for state** - Zoneless change detection (Zone.js not included)
- **DI via `inject()`** - Not constructor injection

### Component Decision Hierarchy

When building UI, follow this order strictly:

1. **PrimeNG Component** - ALWAYS check https://primeng.org first (buttons, inputs, dialogs, dropdowns, menus, tooltips, tables, cards, date pickers, etc.)
2. **Tailwind Utilities** - Style with utility classes only
3. **Custom CSS (LAST RESORT)** - Only for library integration (CDK), animations, or PrimeNG overrides

**Before writing custom CSS, ask:** "Can this be achieved with Tailwind utilities or PrimeNG props?"

### Template Syntax (ENFORCED)

Use Angular's new control flow syntax. The old structural directives are **banned**.

```html
<!-- USE: New control flow -->
@if (loading()) { <spinner /> }
@for (item of items(); track item.id) { ... }
@switch (status()) { @case ('done') { ... } }

<!-- BANNED: Old directives -->
*ngIf, *ngFor, *ngSwitch

<!-- USE: Class bindings -->
[class.active]="isActive()"
[class.text-red-500]="hasError()"

<!-- BANNED: ngClass -->
[ngClass]="{'active': isActive()}"
```

### Signal Patterns

```typescript
// Always mark signal members as readonly
readonly items = signal<Item[]>([]);
readonly count = computed(() => this.items().length);
readonly task = input.required<Task>();
readonly onChange = output<void>();
```

```html
<!-- Form inputs - use native event binding, NOT ngModel -->
<input [value]="mySignal()" (input)="mySignal.set($any($event.target).value)" />

<!-- PrimeNG two-way binding -->
<p-dialog [visible]="showDialog()" (visibleChange)="showDialog.set($event)" />
```

### Signal Forms (Angular 21 - Use for New Forms)

For new form components, use Signal Forms instead of Reactive Forms:

```typescript
// Signal Forms (preferred for new forms)
filter = signal({ from: '', to: '' });
filterForm = form(this.filter, (path) => {
  required(path.from);
  minLength(path.from, 3);
});
```

### Loading/Error/Empty State Pattern

Standardize how views handle async data:

```typescript
// Service signals
readonly loading = signal(false);
readonly error = signal<string | null>(null);
readonly items = signal<Item[]>([]);
```

```html
<!-- Template pattern -->
@if (loading()) {
  <app-skeleton />
} @else if (error()) {
  <p class="text-red-500">{{ error() }}</p>
} @else if (items().length === 0) {
  <p class="text-gray-500 text-center py-8">No items found</p>
} @else {
  @for (item of items(); track item.id) {
    <app-item-card [item]="item" />
  }
}
```

## UX/UI Guidelines

- **Responsive**: All UI must work on mobile devices AND desktop
- **Design**: Clean, modern aesthetic
- **Consistency**: Maintain consistent patterns across the entire application

### Accessibility (Required)

```html
<!-- Always include aria-labels on icon-only buttons -->
<button (click)="delete()" aria-label="Delete bill">
  <i class="pi pi-trash"></i>
</button>

<!-- Use semantic HTML elements -->
<button>...</button>    <!-- Not <div (click)="..."> -->
<nav>...</nav>          <!-- Not <div class="nav"> -->
<main>...</main>        <!-- Not <div class="main"> -->

<!-- Include keyboard navigation for interactive elements -->
(keydown.enter)="action()"
(keydown.escape)="cancel()"
```

### UI Design Resources

- **PrimeNG v21**: https://primeng.org - Components, theming, examples
- **Tailwind CSS v4**: https://tailwindcss.com/docs - Utility classes
- **Angular 21**: https://angular.dev - Official documentation

## Project Structure

```
Backend
├── Domain/           # Aggregates, entities, value objects, repository interfaces
├── Application/      # Use cases, DTOs, commands, queries (vertical slices)
├── Infrastructure/   # EF Core, repository implementations, external services
└── Web/              # Minimal API endpoints, authentication

Frontend (ClientApp)
└── src/app/
    ├── auth/         # Authentication service and models
    ├── home/         # Home/dashboard page
    ├── properties/   # Property management feature
    ├── bills/        # Bill generation and history feature
    └── shared/       # Shared components (if needed)
```

## Naming Conventions

- **Pages**: `*.page.ts` (e.g., `bills.page.ts`)
- **Components**: `*.component.ts` (e.g., `bill-card.component.ts`)
- **Services**: `*.service.ts` (e.g., `bill.service.ts`)
- **Models**: `*.model.ts` (e.g., `bill.model.ts`)
- **Clear naming is critical**: Function and variable names must be descriptive and unambiguous

## Testing Philosophy

### Well-Named Tests Are Essential

Test names should clearly describe what is being tested and the expected outcome. A developer should understand what broke just by reading the test name.

**Format:** `MethodName_Scenario_ExpectedBehavior` or `Should_ExpectedBehavior_When_Condition`

```csharp
// Good - Clear and descriptive
CalculateWaterCost_WithTieredUsage_AppliesCorrectRates()
CalculateElectricity_WithZeroUnits_ReturnsZeroCost()
CreateBill_WithInvalidReadings_ThrowsArgumentException()

// Bad - Vague or meaningless
TestCalculate()
Test1()
WorksCorrectly()
```

### Domain Unit Tests

**Target: ~100% coverage** for the Domain layer. Domain contains core business logic, aggregates, entities, and value objects. These are pure, deterministic, and easy to test—no excuse for gaps.

**DO test:**
- All factory methods (Create, etc.)
- All state-changing methods
- Calculation logic (electricity, water, sanitation, VAT)
- Validation rules (null, empty, invalid values, closing < opening)
- Edge cases and boundary conditions (tier boundaries for water/sanitation)
- Immutability of value objects

**Patterns to follow:**
- AAA pattern (Arrange-Act-Assert)
- Theory tests with InlineData for validation scenarios
- Regions to organize test methods by functionality

### E2E Tests

E2E tests are expensive to write, maintain, and run. Only add E2E tests for **critical flows that would break the business if they fail**.

**DO write E2E tests for:**
- Authentication/authorization flows
- Core business operations (bill creation, PDF export)
- Critical user journeys that span API + UI

**DO NOT write E2E tests for:**
- Individual UI components or styling changes
- New buttons, dialogs, or form fields (unit test these instead)
- Features already covered by existing workflow tests
- Non-critical enhancements

When adding a new feature, ask: "If this breaks, does the app become unusable?" If no, skip the E2E test.

## Development

### Database Migrations

**Migrations merged to main are immutable.** Once merged, a migration may have been pulled by other developers or deployed to any environment.

| Phase | Can Edit? |
|-------|-----------|
| Local branch | Yes - review, customize, test freely |
| In PR (not merged) | Yes - edit based on review feedback |
| Merged to main | Never - create a new migration instead |

To create a new migration:
```bash
cd src/FlatRate.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../FlatRate.Web
```

To remove an unapplied migration (local only):
```bash
dotnet ef migrations remove --startup-project ../FlatRate.Web
```

### Starting the Dev Stack

Run the full stack locally with hot reload (requires Docker):

```bash
docker compose --profile dev-stack up
```

This starts:
- **PostgreSQL** on port 5434
- **.NET API** on port 5003 (with hot reload)
- **Angular** on port 4201 (with hot reload)

Open http://localhost:4201 to develop. Use the mock auth toolbar to log in.

To stop:
```bash
docker compose --profile dev-stack down
```

### Running Tests

```bash
# Unit tests
dotnet test src/FlatRate.slnx

# E2E tests (uses the same PostgreSQL from dev stack)
docker compose --profile e2e up -d --wait
cd tests/FlatRate.E2E.Tests && npm test

# Clean up E2E containers
docker compose --profile e2e down
```

## Git Workflow

### Feature Branches

**ALWAYS** create a new feature branch when working on a new issue or feature. Never commit directly to `main`.

```bash
# Create and switch to a new feature branch
git checkout -b feat/short-description

# Examples:
git checkout -b feat/bill-pdf-export
git checkout -b fix/calculation-rounding-bug
git checkout -b chore/update-dependencies
```

**Branch naming conventions:**
- `feat/` - New features
- `fix/` - Bug fixes
- `chore/` - Maintenance tasks, refactoring, dependencies
- `docs/` - Documentation only changes

### PR Workflow

**ALWAYS** use the `/pr` skill when creating or updating a pull request. This ensures README is reviewed, tests are run, the PR is properly reviewed, and CI checks are monitored for warnings.

**Markdown-only PRs**: When a PR contains ONLY markdown file changes (`.md` files), merge immediately without waiting for CI or review comments. These are documentation-only changes with no runtime impact.

## Domain-Specific: Billing Calculations

FlatRate calculates utility bills with these rules:

### Electricity
- **Type**: Flat rate (single tier)
- **Formula**: `Units × Rate`
- **Unit**: kWh

### Water & Sanitation
- **Type**: Tiered/sliding scale (3 tiers)
- **Tiers**:
  - Tier 1: 0-6 kL
  - Tier 2: 7-15 kL (next 9 kL)
  - Tier 3: 16+ kL (remaining)
- **Unit**: kL

### VAT
- **Rate**: 15% (South African standard)
- **Applied to**: Sum of all utility costs

### Validation Rules
- Closing reading must be >= opening reading
- All rates must be positive numbers
- Billing period end date must be >= start date

## Technical Debt / TODOs

_None currently tracked._

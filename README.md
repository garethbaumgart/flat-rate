# FlatRate

Monthly utility billing for rental properties. Generate electricity, water, and sanitation invoices with tiered tariff calculations and PDF export.

## Features

- **Multi-user property sharing**: Share properties with family members via Google OAuth login
- **Role-based access**: Property owners can invite collaborators (Editors) who can view and create bills
- **Tiered tariff calculations**: Water and sanitation use 3-tier pricing
- **PDF invoice export**: Generate professional invoices for tenants

## Tech Stack

- **Backend**: .NET 10, Entity Framework Core, PostgreSQL
- **Frontend**: Angular 21, PrimeNG, Tailwind CSS
- **Architecture**: Clean Architecture, CQRS, Vertical Slices

## Development

### Prerequisites

- Docker & Docker Compose
- .NET 10 SDK
- Node.js 24

### Running the Dev Stack

```bash
docker compose --profile dev-stack up
```

This starts:
- **PostgreSQL** on port 5432
- **.NET API** on port 5002 (with hot reload)
- **Angular** on port 4200 (with hot reload)

Open http://localhost:4200 to develop.

### Running Tests

```bash
# Unit tests
dotnet test src/FlatRate.slnx

# E2E tests
docker compose --profile e2e up -d --wait
cd tests/FlatRate.E2E.Tests && npm test
docker compose --profile e2e down
```

## Project Structure

```
src/
├── FlatRate.Domain/        # Core business logic, entities, value objects
├── FlatRate.Application/   # Use cases, commands, queries (CQRS)
├── FlatRate.Infrastructure/# EF Core, repositories, external services
└── FlatRate.Web/           # API endpoints, authentication
    └── ClientApp/          # Angular frontend

tests/
└── FlatRate.Domain.Tests/  # Domain unit tests
```

## Deployment

The app deploys automatically to Google Cloud Run when changes are pushed to `main`. The pipeline runs unit and E2E tests before deploying.

See `.github/workflows/deploy-cloud-run.yml` for the full workflow and `setup-gcp.sh` for GCP project setup.

## Billing Calculations

### Electricity
- Flat rate: `Units × Rate`

### Water & Sanitation
- Tiered pricing:
  - Tier 1: 0-6 kL
  - Tier 2: 7-15 kL
  - Tier 3: 16+ kL

### VAT
- 15% applied to subtotal

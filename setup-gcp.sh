#!/bin/bash
set -euo pipefail

# ============================================================
# Flat-Rate GCP Setup Script
# Run: gcloud auth login   (first, if session expired)
# Then: bash setup-gcp.sh
# ============================================================

PROJECT_ID="flat-rate-486409"
REGION="australia-southeast1"
SERVICE_ACCOUNT_NAME="flatrate-github-deployer"
REPO_NAME="flatrate"
POOL_NAME="github-actions"
PROVIDER_NAME="github"
GITHUB_REPO="garethbaumgart/flat-rate"

echo "=== Setting project ==="
gcloud config set project "$PROJECT_ID"

# ============================================================
# 1. Enable required APIs
# ============================================================
echo ""
echo "=== Enabling APIs ==="
gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  secretmanager.googleapis.com \
  iam.googleapis.com \
  iamcredentials.googleapis.com \
  cloudresourcemanager.googleapis.com

# ============================================================
# 2. Create Artifact Registry repository
# ============================================================
echo ""
echo "=== Creating Artifact Registry repository ==="
gcloud artifacts repositories create "$REPO_NAME" \
  --repository-format=docker \
  --location="$REGION" \
  --description="FlatRate Docker images" \
  2>/dev/null || echo "  (repository already exists)"

# ============================================================
# 3. Create Service Account
# ============================================================
echo ""
echo "=== Creating Service Account ==="
gcloud iam service-accounts create "$SERVICE_ACCOUNT_NAME" \
  --display-name="FlatRate GitHub Actions Deployer" \
  2>/dev/null || echo "  (service account already exists)"

SA_EMAIL="${SERVICE_ACCOUNT_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

echo ""
echo "=== Granting IAM roles ==="
for ROLE in \
  roles/run.admin \
  roles/artifactregistry.writer \
  roles/iam.serviceAccountUser \
  roles/secretmanager.secretAccessor; do
  gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:${SA_EMAIL}" \
    --role="$ROLE" \
    --condition=None \
    --quiet
done

# ============================================================
# 4. Create Workload Identity Federation pool + provider
# ============================================================
echo ""
echo "=== Creating Workload Identity Pool ==="
gcloud iam workload-identity-pools create "$POOL_NAME" \
  --location="global" \
  --display-name="GitHub Actions" \
  2>/dev/null || echo "  (pool already exists)"

echo ""
echo "=== Creating OIDC Provider ==="
gcloud iam workload-identity-pools providers create-oidc "$PROVIDER_NAME" \
  --location="global" \
  --workload-identity-pool="$POOL_NAME" \
  --display-name="GitHub" \
  --attribute-mapping="google.subject=assertion.sub,attribute.repository=assertion.repository" \
  --attribute-condition="assertion.repository == \"${GITHUB_REPO}\"" \
  --issuer-uri="https://token.actions.githubusercontent.com" \
  2>/dev/null || echo "  (provider already exists)"

# ============================================================
# 5. Bind service account to workload identity pool
# ============================================================
echo ""
echo "=== Binding service account to Workload Identity Pool ==="
PROJECT_NUMBER=$(gcloud projects describe "$PROJECT_ID" --format="value(projectNumber)")

gcloud iam service-accounts add-iam-policy-binding "$SA_EMAIL" \
  --role="roles/iam.workloadIdentityUser" \
  --member="principalSet://iam.googleapis.com/projects/${PROJECT_NUMBER}/locations/global/workloadIdentityPools/${POOL_NAME}/attribute.repository/${GITHUB_REPO}" \
  --quiet

# ============================================================
# 6. Create Secret Manager secrets (empty — you fill values next)
# ============================================================
echo ""
echo "=== Creating Secret Manager secrets ==="
for SECRET in CONNECTION_STRING GOOGLE_CLIENT_ID GOOGLE_CLIENT_SECRET; do
  gcloud secrets create "$SECRET" --replication-policy="automatic" \
    2>/dev/null || echo "  ($SECRET already exists)"
done

# ============================================================
# 7. Print the values you need for GitHub Secrets
# ============================================================
WIF_PROVIDER=$(gcloud iam workload-identity-pools providers describe "$PROVIDER_NAME" \
  --location="global" \
  --workload-identity-pool="$POOL_NAME" \
  --format="value(name)")

echo ""
echo "============================================================"
echo "  GCP SETUP COMPLETE"
echo "============================================================"
echo ""
echo "Add these as GitHub Actions secrets at:"
echo "  https://github.com/${GITHUB_REPO}/settings/secrets/actions"
echo ""
echo "  GCP_PROJECT_ID:"
echo "    ${PROJECT_ID}"
echo ""
echo "  GCP_SERVICE_ACCOUNT:"
echo "    ${SA_EMAIL}"
echo ""
echo "  GCP_WORKLOAD_IDENTITY_PROVIDER:"
echo "    ${WIF_PROVIDER}"
echo ""
echo "============================================================"
echo ""
echo "NEXT STEPS:"
echo ""
echo "1. Add the three GitHub secrets above"
echo ""
echo "2. Create your Neon database at https://console.neon.tech"
echo "   then add the connection string:"
echo ""
echo "   gcloud secrets versions add CONNECTION_STRING --data-file=-"
echo "   (paste your connection string, then press Ctrl+D)"
echo ""
echo "3. Create Google OAuth credentials at:"
echo "   https://console.cloud.google.com/apis/credentials?project=${PROJECT_ID}"
echo "   then add them:"
echo ""
echo "   gcloud secrets versions add GOOGLE_CLIENT_ID --data-file=-"
echo "   gcloud secrets versions add GOOGLE_CLIENT_SECRET --data-file=-"
echo ""
echo "============================================================"

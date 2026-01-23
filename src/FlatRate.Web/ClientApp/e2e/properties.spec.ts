import { test, expect } from '@playwright/test';

test.describe('Property Management', () => {
  test.beforeEach(async ({ page }) => {
    // Set mock auth header for all requests
    await page.setExtraHTTPHeaders({
      'X-Mock-User': 'e2e-test-user'
    });
  });

  test('should display properties page', async ({ page }) => {
    await page.goto('/properties');

    // Should see the properties page title
    await expect(page.getByRole('heading', { name: /Properties/i })).toBeVisible();
  });

  test('should navigate to add property page', async ({ page }) => {
    await page.goto('/properties');

    // Click the Add Property button
    await page.getByRole('button', { name: /Add Property/i }).click();

    // Should be on the add property page
    await expect(page.getByRole('heading', { name: /Add Property/i })).toBeVisible();
  });

  test('should create a new property', async ({ page }) => {
    await page.goto('/properties/add');

    // Fill in the form
    await page.getByLabel(/Name/i).fill('E2E Test Property');
    await page.getByLabel(/Address/i).fill('123 Test Street');

    // Fill in default rates
    await page.getByLabel(/Electricity Rate/i).fill('2.50');
    await page.getByLabel(/Water Rate.*Tier 1/i).first().fill('10.00');
    await page.getByLabel(/Water Rate.*Tier 2/i).first().fill('15.00');
    await page.getByLabel(/Water Rate.*Tier 3/i).first().fill('20.00');
    await page.getByLabel(/Sanitation Rate.*Tier 1/i).first().fill('8.00');
    await page.getByLabel(/Sanitation Rate.*Tier 2/i).first().fill('12.00');
    await page.getByLabel(/Sanitation Rate.*Tier 3/i).first().fill('16.00');

    // Submit the form
    await page.getByRole('button', { name: /Save/i }).click();

    // Should redirect back to properties page
    await expect(page).toHaveURL(/\/properties$/);

    // Should see the new property in the list
    await expect(page.getByText('E2E Test Property')).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    await page.goto('/properties/add');

    // Try to submit without filling required fields
    await page.getByRole('button', { name: /Save/i }).click();

    // Form should not submit (button should still be visible)
    await expect(page).toHaveURL(/\/properties\/add$/);
  });
});

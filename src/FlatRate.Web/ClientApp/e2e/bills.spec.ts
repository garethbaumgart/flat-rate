import { test, expect } from '@playwright/test';

test.describe('Bill Management', () => {
  test.beforeEach(async ({ page }) => {
    // Set mock auth header for all requests
    await page.setExtraHTTPHeaders({
      'X-Mock-User': 'e2e-test-user'
    });
  });

  test('should display bills page', async ({ page }) => {
    await page.goto('/bills');

    // Should see the bills page title
    await expect(page.getByRole('heading', { name: /Bills/i })).toBeVisible();
  });

  test('should navigate to create bill page', async ({ page }) => {
    await page.goto('/bills');

    // Click the Create Bill button
    await page.getByRole('button', { name: /Create Bill/i }).click();

    // Should be on the create bill page
    await expect(page.getByRole('heading', { name: /Create Bill/i })).toBeVisible();
  });

  test('should show live preview when filling bill form', async ({ page }) => {
    // First create a property via API
    const propertyResponse = await page.request.post('/api/properties', {
      headers: {
        'X-Mock-User': 'e2e-test-user'
      },
      data: {
        name: 'E2E Bill Test Property',
        address: '456 Bill Street',
        defaultElectricityRate: 2.50,
        defaultWaterRateTier1: 10.00,
        defaultWaterRateTier2: 15.00,
        defaultWaterRateTier3: 20.00,
        defaultSanitationRateTier1: 8.00,
        defaultSanitationRateTier2: 12.00,
        defaultSanitationRateTier3: 16.00
      }
    });
    expect(propertyResponse.ok()).toBeTruthy();

    await page.goto('/bills/create');

    // Select the property (wait for it to be visible)
    const propertySelect = page.locator('p-select').first();
    await expect(propertySelect).toBeVisible();
    await propertySelect.click();
    await page.getByText('E2E Bill Test Property').click();

    // Fill in billing period
    const startDatePicker = page.locator('p-datepicker').first();
    await startDatePicker.click();
    await page.getByRole('button', { name: '1' }).first().click();

    const endDatePicker = page.locator('p-datepicker').nth(1);
    await endDatePicker.click();
    await page.getByRole('button', { name: '28' }).first().click();

    // Fill in meter readings
    await page.getByLabel(/Electricity Opening/i).fill('100');
    await page.getByLabel(/Electricity Closing/i).fill('200');
    await page.getByLabel(/Water Opening/i).fill('50');
    await page.getByLabel(/Water Closing/i).fill('70');
    await page.getByLabel(/Sanitation Opening/i).fill('50');
    await page.getByLabel(/Sanitation Closing/i).fill('70');

    // The preview should show calculated values
    await expect(page.getByText(/100.00 kWh/)).toBeVisible();
    await expect(page.getByText(/20.00 kL/)).toBeVisible();
  });

  test('should show empty state when no bills exist', async ({ page }) => {
    await page.goto('/bills');

    // Wait for loading to complete
    await page.waitForLoadState('networkidle');

    // Should show either the bills table or empty state
    const hasBills = await page.getByRole('table').isVisible().catch(() => false);
    if (!hasBills) {
      await expect(page.getByText(/No bills found/i)).toBeVisible();
    }
  });
});

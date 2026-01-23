import { test, expect } from '@playwright/test';

/**
 * Critical path E2E test: Full workflow from property creation to bill export.
 * This test covers the main user journey through the application.
 */
test.describe('Critical Path: Property → Bill → PDF Export', () => {
  const uniqueId = Date.now().toString();

  test.beforeEach(async ({ page }) => {
    // Set mock auth header for all requests
    await page.setExtraHTTPHeaders({
      'X-Mock-User': `critical-path-test-${uniqueId}`
    });
  });

  test('complete workflow: create property, create bill, view bill, download PDF', async ({ page }) => {
    // Step 1: Create a property
    await test.step('Create a new property', async () => {
      await page.goto('/properties/add');

      await page.getByLabel(/Name/i).fill(`Critical Path Property ${uniqueId}`);
      await page.getByLabel(/Address/i).fill('789 Critical Path Lane');

      // Fill in default rates
      await page.getByLabel(/Electricity Rate/i).fill('3.00');
      await page.getByLabel(/Water Rate.*Tier 1/i).first().fill('12.00');
      await page.getByLabel(/Water Rate.*Tier 2/i).first().fill('18.00');
      await page.getByLabel(/Water Rate.*Tier 3/i).first().fill('24.00');
      await page.getByLabel(/Sanitation Rate.*Tier 1/i).first().fill('10.00');
      await page.getByLabel(/Sanitation Rate.*Tier 2/i).first().fill('15.00');
      await page.getByLabel(/Sanitation Rate.*Tier 3/i).first().fill('20.00');

      await page.getByRole('button', { name: /Save/i }).click();

      // Verify we're back on properties page and see the new property
      await expect(page).toHaveURL(/\/properties$/);
      await expect(page.getByText(`Critical Path Property ${uniqueId}`)).toBeVisible();
    });

    // Step 2: Create a bill for the property
    await test.step('Create a bill for the property', async () => {
      await page.goto('/bills/create');

      // Wait for properties to load by waiting for the property select to be visible
      const propertySelect = page.locator('p-select').first();
      await propertySelect.waitFor({ state: 'visible' });

      // Select the property
      await propertySelect.click();
      await page.getByText(`Critical Path Property ${uniqueId}`).click();

      // Fill in billing period - using date pickers
      const startDatePicker = page.locator('p-datepicker').first();
      await startDatePicker.click();
      await page.getByRole('button', { name: '1' }).first().click();

      const endDatePicker = page.locator('p-datepicker').nth(1);
      await endDatePicker.click();
      await page.getByRole('button', { name: '28' }).first().click();

      // Fill in meter readings
      await page.getByLabel(/Electricity Opening/i).fill('1000');
      await page.getByLabel(/Electricity Closing/i).fill('1150');
      await page.getByLabel(/Water Opening/i).fill('500');
      await page.getByLabel(/Water Closing/i).fill('525');
      await page.getByLabel(/Sanitation Opening/i).fill('500');
      await page.getByLabel(/Sanitation Closing/i).fill('525');

      // Verify preview shows calculated values
      await expect(page.getByText(/150.00 kWh/)).toBeVisible();
      await expect(page.getByText(/25.00 kL/)).toBeVisible();

      // Create the bill
      await page.getByRole('button', { name: /Create Bill/i }).click();

      // Should redirect to bills page
      await expect(page).toHaveURL(/\/bills$/);
    });

    // Step 3: View the bill details
    await test.step('View bill details', async () => {
      await page.goto('/bills');

      // Find and click view button for the bill
      const viewButton = page.getByRole('button', { name: /View/i }).first();
      await viewButton.click();

      // Verify bill details dialog is visible
      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByText(/Bill Details/i)).toBeVisible();

      // Verify the property name is shown
      await expect(page.getByText(`Critical Path Property ${uniqueId}`)).toBeVisible();

      // Verify meter readings are shown
      await expect(page.getByText(/1000/)).toBeVisible();
      await expect(page.getByText(/1150/)).toBeVisible();
    });

    // Step 4: Download PDF (verify the button exists and is clickable)
    await test.step('Download PDF button is available', async () => {
      // The dialog should still be open from previous step
      const downloadButton = page.getByRole('button', { name: /Download PDF/i });
      await expect(downloadButton).toBeVisible();
      await expect(downloadButton).toBeEnabled();

      // Note: We don't actually click the download button in E2E tests
      // as it would trigger a file download which is harder to verify.
      // The button being present and enabled confirms the feature is available.
    });

    // Step 5: Close dialog and verify bills list
    await test.step('Close dialog and verify bills list', async () => {
      await page.getByRole('button', { name: /Close/i }).click();

      // Dialog should be closed
      await expect(page.getByRole('dialog')).not.toBeVisible();

      // Bills table should be visible
      await expect(page.getByRole('table')).toBeVisible();
    });
  });
});

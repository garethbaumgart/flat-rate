import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for E2E tests.
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,
  reporter: process.env['CI']
    ? [['github'], ['html', { outputFolder: 'playwright-report' }]]
    : 'html',

  use: {
    baseURL: 'http://localhost:5297',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'cd .. && dotnet run',
    url: 'http://localhost:5297/api/health',
    reuseExistingServer: !process.env['CI'],
    timeout: 180000,
    env: {
      ASPNETCORE_ENVIRONMENT: 'Development',
    },
  },
});

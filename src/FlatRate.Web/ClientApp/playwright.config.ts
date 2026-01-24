import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for E2E tests.
 * @see https://playwright.dev/docs/test-configuration
 */

// Use BASE_URL from Docker environment, or default to local dev server
const baseURL = process.env['BASE_URL'] || 'http://localhost:5297';

// In Docker E2E (CI), we don't need webServer - the API is already running
const isDockerE2E = !!process.env['BASE_URL'];

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,
  reporter: process.env['CI'] || isDockerE2E
    ? [['github'], ['html', { outputFolder: 'playwright-report' }]]
    : 'html',

  use: {
    baseURL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Only use webServer for local development (not in Docker E2E)
  ...(isDockerE2E ? {} : {
    webServer: {
      command: 'cd .. && dotnet run',
      url: 'http://localhost:5297/api/health',
      reuseExistingServer: true,
      timeout: 180000,
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'Development',
      },
    },
  }),
});

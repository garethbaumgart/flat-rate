import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('should show login button when not authenticated', async ({ page }) => {
    await page.goto('/');

    // The page should load without errors
    await expect(page).toHaveTitle(/FlatRate/);

    // Should see the home page
    await expect(page.getByRole('heading', { name: /Welcome to FlatRate/i })).toBeVisible();
  });

  test('should access protected route when authenticated via mock auth', async ({ page }) => {
    // Set mock auth header for all requests
    await page.setExtraHTTPHeaders({
      'X-Mock-User': 'test-user-123'
    });

    await page.goto('/');

    // Should be able to see the home page
    await expect(page.getByRole('heading', { name: /Welcome to FlatRate/i })).toBeVisible();
  });

  test('should return user info when authenticated', async ({ request }) => {
    const response = await request.get('/api/auth/user', {
      headers: {
        'X-Mock-User': 'test-user-123'
      }
    });

    expect(response.status()).toBe(200);
    const user = await response.json();
    expect(user.id).toBe('test-user-123');
    expect(user.email).toBe('test-user-123@mock.local');
  });

  test('should return 401 when not authenticated', async ({ request }) => {
    const response = await request.get('/api/auth/user');
    expect(response.status()).toBe(401);
  });
});

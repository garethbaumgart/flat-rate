import { test, expect } from '@playwright/test';

test.describe('Authentication API', () => {
  test('should return user info when authenticated', async ({ request }) => {
    const response = await request.get('/api/auth/user', {
      headers: {
        'X-Mock-User': 'test-user-123'
      }
    });

    expect(response.status()).toBe(200);
    const user = await response.json();
    // The id is now an internal GUID, not the Google ID
    expect(user.id).toBeDefined();
    expect(typeof user.id).toBe('string');
    expect(user.id.length).toBe(36); // GUID format
    expect(user.googleId).toBe('test-user-123');
    expect(user.email).toBe('test-user-123@mock.local');
  });

  test('should return 302 redirect when not authenticated', async ({ request }) => {
    // When not authenticated, the endpoint redirects to login
    const response = await request.get('/api/auth/user', {
      maxRedirects: 0
    });
    // 302 redirect to login or 401 depending on configuration
    expect([302, 401]).toContain(response.status());
  });
});

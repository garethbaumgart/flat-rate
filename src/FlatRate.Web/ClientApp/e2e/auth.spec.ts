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
    expect(user.id).toBe('test-user-123');
    expect(user.email).toBe('test-user-123@mock.local');
  });

  test('should return 401 when not authenticated', async ({ request }) => {
    const response = await request.get('/api/auth/user');
    expect(response.status()).toBe(401);
  });
});

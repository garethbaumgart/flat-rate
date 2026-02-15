import { test, expect } from '@playwright/test';

/**
 * E2E tests covering the full property lifecycle:
 * create property → create bill → export PDF → share with collaborator → revoke access.
 * Also verifies that sharing is restricted to registered users.
 * Tests run serially to ensure consistent state across dependent steps.
 */
test.describe.configure({ mode: 'serial' });

test.describe('Property Lifecycle', () => {
  const ownerAuth = { 'X-Mock-User': 'lifecycle-owner' };
  const collaboratorAuth = { 'X-Mock-User': 'lifecycle-collaborator' };
  let propertyId: string;
  let billId: string;
  let collaboratorUserId: string;

  // Helper to ensure user exists before making authenticated API calls
  async function ensureUserExists(
    request: any,
    headers: Record<string, string>
  ) {
    const response = await request.get('/api/auth/user', { headers });
    expect(response.status()).toBe(200);
    return response.json();
  }

  test('owner creates a property', async ({ request }) => {
    await ensureUserExists(request, ownerAuth);

    const res = await request.post('/api/properties', {
      headers: ownerAuth,
      data: {
        name: 'Lifecycle Property',
        address: '100 Test Street',
      },
    });

    expect(res.status()).toBe(201);
    const body = await res.json();
    propertyId = body.id;
    expect(propertyId).toBeDefined();
  });

  test('owner creates a bill and verifies it appears in list', async ({
    request,
  }) => {
    const res = await request.post('/api/bills', {
      headers: ownerAuth,
      data: {
        propertyId,
        periodStart: '2026-01-01',
        periodEnd: '2026-01-31',
        electricityReadingOpening: 1000,
        electricityReadingClosing: 1500,
        waterReadingOpening: 100,
        waterReadingClosing: 120,
        sanitationReadingOpening: 100,
        sanitationReadingClosing: 120,
        electricityRate: 2.5,
        waterRateTier1: 10.0,
        waterRateTier2: 15.0,
        waterRateTier3: 20.0,
        sanitationRateTier1: 8.0,
        sanitationRateTier2: 12.0,
        sanitationRateTier3: 16.0,
      },
    });

    expect(res.status()).toBe(201);
    const body = await res.json();
    billId = body.id;
    expect(billId).toBeDefined();

    // Verify bill appears in list
    const listRes = await request.get(
      `/api/bills?propertyId=${propertyId}`,
      { headers: ownerAuth }
    );
    expect(listRes.status()).toBe(200);
    const bills = await listRes.json();
    expect(bills.some((b: any) => b.id === billId)).toBeTruthy();
  });

  test('owner can download bill as PDF', async ({ request }) => {
    const res = await request.get(`/api/bills/${billId}/pdf`, {
      headers: ownerAuth,
    });

    expect(res.status()).toBe(200);
    expect(res.headers()['content-type']).toBe('application/pdf');
  });

  test('owner shares property with a registered user', async ({
    request,
  }) => {
    const collaborator = await ensureUserExists(request, collaboratorAuth);
    collaboratorUserId = collaborator.id;

    const res = await request.post(
      `/api/properties/${propertyId}/collaborators`,
      {
        headers: ownerAuth,
        data: {
          email: collaborator.email,
        },
      }
    );

    expect(res.status()).toBe(200);

    // Verify collaborator appears in list
    const collabRes = await request.get(
      `/api/properties/${propertyId}/collaborators`,
      { headers: ownerAuth }
    );
    expect(collabRes.status()).toBe(200);
    const collaborators = await collabRes.json();
    expect(
      collaborators.some((c: any) => c.userId === collaboratorUserId)
    ).toBeTruthy();
  });

  test('collaborator can access the shared property and its bills', async ({
    request,
  }) => {
    const propsRes = await request.get('/api/properties', {
      headers: collaboratorAuth,
    });
    expect(propsRes.status()).toBe(200);
    const props = await propsRes.json();
    expect(props.some((p: any) => p.id === propertyId)).toBeTruthy();

    const billsRes = await request.get(
      `/api/bills?propertyId=${propertyId}`,
      { headers: collaboratorAuth }
    );
    expect(billsRes.status()).toBe(200);
    const bills = await billsRes.json();
    expect(bills.some((b: any) => b.id === billId)).toBeTruthy();
  });

  test('sharing fails for unregistered email with clear error', async ({
    request,
  }) => {
    const res = await request.post(
      `/api/properties/${propertyId}/collaborators`,
      {
        headers: ownerAuth,
        data: { email: 'nobody-does-not-exist@example.com' },
      }
    );

    expect(res.status()).toBe(400);
    const body = await res.json();
    expect(body.error).toContain('No account found');
  });

  test('owner revokes collaborator access', async ({ request }) => {
    const res = await request.delete(
      `/api/properties/${propertyId}/collaborators/${collaboratorUserId}`,
      { headers: ownerAuth }
    );

    expect(res.status()).toBe(200);

    // Verify collaborator no longer sees the property
    const propsRes = await request.get('/api/properties', {
      headers: collaboratorAuth,
    });
    expect(propsRes.status()).toBe(200);
    const props = await propsRes.json();
    expect(props.some((p: any) => p.id === propertyId)).toBeFalsy();
  });
});

import { test, expect } from '@playwright/test';

/**
 * E2E tests for DateTimeOffset handling across the API.
 * Verifies that ISO 8601 dates with timezone offsets round-trip correctly.
 */
test.describe.configure({ mode: 'serial' });

test.describe('DateTimeOffset API Handling', () => {
  const mockAuthHeader = { 'X-Mock-User': 'datetime-test-user' };

  async function ensureUserExists(request: any) {
    const response = await request.get('/api/auth/user', {
      headers: mockAuthHeader
    });
    expect(response.status()).toBe(200);
  }

  async function createProperty(request: any, suffix: string = ''): Promise<string> {
    const response = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: `DateTime Test Property ${suffix || Date.now()}`,
        address: `123 DateTime St ${suffix || Date.now()}`,
        defaultElectricityRate: 3.40,
        defaultWaterRateTier1: 20.80,
        defaultWaterRateTier2: 34.20,
        defaultWaterRateTier3: 48.50,
        defaultSanitationRateTier1: 25.50,
        defaultSanitationRateTier2: 20.50,
        defaultSanitationRateTier3: 29.80
      }
    });
    expect(response.status()).toBe(201);
    const data = await response.json();
    return data.id;
  }

  test('health endpoint should return ISO 8601 timestamp with timezone info', async ({ request }) => {
    const response = await request.get('/api/health');
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data.timestamp).toBeDefined();

    // DateTimeOffset serializes with timezone offset (e.g., +00:00 or Z)
    const timestamp = data.timestamp as string;
    const hasTimezoneInfo = timestamp.includes('+') || timestamp.endsWith('Z');
    expect(hasTimezoneInfo).toBeTruthy();
  });

  test('API should accept ISO 8601 UTC dates and round-trip correctly', async ({ request }) => {
    await ensureUserExists(request);
    const propertyId = await createProperty(request);

    // POST with explicit UTC dates
    const createResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId,
        periodStart: '2024-04-01T00:00:00Z',
        periodEnd: '2024-04-30T00:00:00Z',
        electricityReadingOpening: 12720,
        electricityReadingClosing: 12850,
        waterReadingOpening: 222,
        waterReadingClosing: 230,
        sanitationReadingOpening: 222,
        sanitationReadingClosing: 230,
        electricityRate: 3.40,
        waterRateTier1: 20.80,
        waterRateTier2: 34.20,
        waterRateTier3: 48.50,
        sanitationRateTier1: 25.50,
        sanitationRateTier2: 20.50,
        sanitationRateTier3: 29.80
      }
    });

    if (createResponse.status() !== 201) {
      const errorBody = await createResponse.text();
      console.error('UTC bill creation failed:', createResponse.status(), errorBody);
    }
    expect(createResponse.status()).toBe(201);
    const { id } = await createResponse.json();

    // GET and verify dates round-trip
    const getResponse = await request.get(`/api/bills/${id}`, {
      headers: mockAuthHeader
    });
    expect(getResponse.status()).toBe(200);

    const bill = await getResponse.json();
    // Verify the dates are parseable and represent the correct date
    const periodStart = new Date(bill.periodStart);
    const periodEnd = new Date(bill.periodEnd);
    expect(periodStart.toISOString()).toBe('2024-04-01T00:00:00.000Z');
    expect(periodEnd.toISOString()).toBe('2024-04-30T00:00:00.000Z');

    // Verify createdAt has timezone info
    const createdAt = bill.createdAt as string;
    const hasTimezoneInfo = createdAt.includes('+') || createdAt.endsWith('Z');
    expect(hasTimezoneInfo).toBeTruthy();
  });

  test('API should accept ISO 8601 with positive timezone offset', async ({ request }) => {
    await ensureUserExists(request);
    const propertyId = await createProperty(request);

    // POST with South Africa timezone (UTC+2)
    const createResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId,
        periodStart: '2024-06-01T00:00:00+02:00',
        periodEnd: '2024-06-30T00:00:00+02:00',
        electricityReadingOpening: 12720,
        electricityReadingClosing: 12850,
        waterReadingOpening: 222,
        waterReadingClosing: 230,
        sanitationReadingOpening: 222,
        sanitationReadingClosing: 230,
        electricityRate: 3.40,
        waterRateTier1: 20.80,
        waterRateTier2: 34.20,
        waterRateTier3: 48.50,
        sanitationRateTier1: 25.50,
        sanitationRateTier2: 20.50,
        sanitationRateTier3: 29.80
      }
    });

    if (createResponse.status() !== 201) {
      const errorBody = await createResponse.text();
      console.error('Bill creation failed:', createResponse.status(), errorBody);
    }
    expect(createResponse.status()).toBe(201);
    const { id } = await createResponse.json();

    // GET and verify dates were accepted
    const getResponse = await request.get(`/api/bills/${id}`, {
      headers: mockAuthHeader
    });
    expect(getResponse.status()).toBe(200);

    const bill = await getResponse.json();
    // The date should be stored and returned - UTC+2 midnight = UTC 22:00 previous day
    const periodStart = new Date(bill.periodStart);
    expect(periodStart.toISOString()).toBe('2024-05-31T22:00:00.000Z');
  });

  test('API should accept date-only strings', async ({ request }) => {
    await ensureUserExists(request);
    const propertyId = await createProperty(request);

    // POST with date-only strings (no time component)
    const createResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId,
        periodStart: '2024-04-01',
        periodEnd: '2024-04-30',
        electricityReadingOpening: 12720,
        electricityReadingClosing: 12850,
        waterReadingOpening: 222,
        waterReadingClosing: 230,
        sanitationReadingOpening: 222,
        sanitationReadingClosing: 230,
        electricityRate: 3.40,
        waterRateTier1: 20.80,
        waterRateTier2: 34.20,
        waterRateTier3: 48.50,
        sanitationRateTier1: 25.50,
        sanitationRateTier2: 20.50,
        sanitationRateTier3: 29.80
      }
    });

    expect(createResponse.status()).toBe(201);
    const { id } = await createResponse.json();

    // Verify the bill was created and can be retrieved
    const getResponse = await request.get(`/api/bills/${id}`, {
      headers: mockAuthHeader
    });
    expect(getResponse.status()).toBe(200);

    const bill = await getResponse.json();
    expect(bill.periodStart).toBeDefined();
    expect(bill.periodEnd).toBeDefined();
  });

  test('PDF download should work after DateTimeOffset migration', async ({ request }) => {
    await ensureUserExists(request);
    const propertyId = await createProperty(request);

    // Create a bill with ISO 8601 dates
    const createResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId,
        periodStart: '2024-04-01T00:00:00Z',
        periodEnd: '2024-04-30T00:00:00Z',
        electricityReadingOpening: 12720,
        electricityReadingClosing: 12850,
        waterReadingOpening: 222,
        waterReadingClosing: 230,
        sanitationReadingOpening: 222,
        sanitationReadingClosing: 230,
        electricityRate: 3.40,
        waterRateTier1: 20.80,
        waterRateTier2: 34.20,
        waterRateTier3: 48.50,
        sanitationRateTier1: 25.50,
        sanitationRateTier2: 20.50,
        sanitationRateTier3: 29.80
      }
    });

    expect(createResponse.status()).toBe(201);
    const { id } = await createResponse.json();

    // Download the PDF
    const pdfResponse = await request.get(`/api/bills/${id}/pdf`, {
      headers: mockAuthHeader
    });

    expect(pdfResponse.status()).toBe(200);
    expect(pdfResponse.headers()['content-type']).toBe('application/pdf');
  });
});

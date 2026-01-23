import { test, expect } from '@playwright/test';

/**
 * API-level E2E tests that verify the backend endpoints work correctly.
 * These tests don't require the Angular frontend to be served.
 */
test.describe('API Endpoints', () => {
  const mockAuthHeader = { 'X-Mock-User': 'api-test-user' };
  let createdPropertyId: string;
  let createdBillId: string;

  test('health endpoint should return healthy status', async ({ request }) => {
    const response = await request.get('/api/health');
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data.status).toBe('healthy');
    expect(data.timestamp).toBeDefined();
  });

  test('should create a property', async ({ request }) => {
    const response = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: 'API Test Property',
        address: '123 API Street',
        defaultElectricityRate: 2.50,
        defaultWaterRateTier1: 10.00,
        defaultWaterRateTier2: 15.00,
        defaultWaterRateTier3: 20.00,
        defaultSanitationRateTier1: 8.00,
        defaultSanitationRateTier2: 12.00,
        defaultSanitationRateTier3: 16.00
      }
    });

    expect(response.status()).toBe(201);
    const data = await response.json();
    expect(data.id).toBeDefined();
    createdPropertyId = data.id;
  });

  test('should list properties', async ({ request }) => {
    const response = await request.get('/api/properties', {
      headers: mockAuthHeader
    });

    expect(response.status()).toBe(200);
    const data = await response.json();
    expect(Array.isArray(data)).toBeTruthy();
  });

  test('should get property by id', async ({ request }) => {
    // First create a property
    const createResponse = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: 'Get By ID Test',
        address: '456 Test Ave',
        defaultElectricityRate: 3.00,
        defaultWaterRateTier1: 11.00,
        defaultWaterRateTier2: 16.00,
        defaultWaterRateTier3: 21.00,
        defaultSanitationRateTier1: 9.00,
        defaultSanitationRateTier2: 13.00,
        defaultSanitationRateTier3: 17.00
      }
    });

    expect(createResponse.status()).toBe(201);
    const created = await createResponse.json();

    // Then get it by ID
    const response = await request.get(`/api/properties/${created.id}`, {
      headers: mockAuthHeader
    });

    expect(response.status()).toBe(200);
    const data = await response.json();
    expect(data.name).toBe('Get By ID Test');
    expect(data.address).toBe('456 Test Ave');
  });

  test('should create a bill', async ({ request }) => {
    // First create a property
    const propertyResponse = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: 'Bill Test Property',
        address: '789 Bill Lane',
        defaultElectricityRate: 2.50,
        defaultWaterRateTier1: 10.00,
        defaultWaterRateTier2: 15.00,
        defaultWaterRateTier3: 20.00,
        defaultSanitationRateTier1: 8.00,
        defaultSanitationRateTier2: 12.00,
        defaultSanitationRateTier3: 16.00
      }
    });

    expect(propertyResponse.status()).toBe(201);
    const property = await propertyResponse.json();

    // Create a bill
    const response = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId: property.id,
        periodStart: '2024-01-01',
        periodEnd: '2024-01-31',
        electricityOpening: 1000,
        electricityClosing: 1150,
        electricityRate: 2.50,
        waterOpening: 500,
        waterClosing: 525,
        waterRateTier1: 10.00,
        waterRateTier2: 15.00,
        waterRateTier3: 20.00,
        sanitationOpening: 500,
        sanitationClosing: 525,
        sanitationRateTier1: 8.00,
        sanitationRateTier2: 12.00,
        sanitationRateTier3: 16.00
      }
    });

    expect(response.status()).toBe(201);
    const data = await response.json();
    expect(data.id).toBeDefined();
    createdBillId = data.id;
  });

  test('should list bills', async ({ request }) => {
    const response = await request.get('/api/bills', {
      headers: mockAuthHeader
    });

    expect(response.status()).toBe(200);
    const data = await response.json();
    expect(Array.isArray(data)).toBeTruthy();
  });

  test('should get bill by id', async ({ request }) => {
    // First create a property
    const propertyResponse = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: 'Get Bill Test Property',
        address: '111 Get Bill St',
        defaultElectricityRate: 2.50,
        defaultWaterRateTier1: 10.00,
        defaultWaterRateTier2: 15.00,
        defaultWaterRateTier3: 20.00,
        defaultSanitationRateTier1: 8.00,
        defaultSanitationRateTier2: 12.00,
        defaultSanitationRateTier3: 16.00
      }
    });

    const property = await propertyResponse.json();

    // Create a bill
    const billResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId: property.id,
        periodStart: '2024-02-01',
        periodEnd: '2024-02-28',
        electricityOpening: 1150,
        electricityClosing: 1300,
        electricityRate: 2.50,
        waterOpening: 525,
        waterClosing: 550,
        waterRateTier1: 10.00,
        waterRateTier2: 15.00,
        waterRateTier3: 20.00,
        sanitationOpening: 525,
        sanitationClosing: 550,
        sanitationRateTier1: 8.00,
        sanitationRateTier2: 12.00,
        sanitationRateTier3: 16.00
      }
    });

    const bill = await billResponse.json();

    // Get the bill by ID
    const response = await request.get(`/api/bills/${bill.id}`, {
      headers: mockAuthHeader
    });

    expect(response.status()).toBe(200);
    const data = await response.json();
    expect(data.propertyId).toBe(property.id);
    expect(data.invoiceNumber).toBeDefined();
  });

  test('should download bill PDF', async ({ request }) => {
    // First create a property
    const propertyResponse = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: 'PDF Test Property',
        address: '222 PDF Ave',
        defaultElectricityRate: 2.50,
        defaultWaterRateTier1: 10.00,
        defaultWaterRateTier2: 15.00,
        defaultWaterRateTier3: 20.00,
        defaultSanitationRateTier1: 8.00,
        defaultSanitationRateTier2: 12.00,
        defaultSanitationRateTier3: 16.00
      }
    });

    const property = await propertyResponse.json();

    // Create a bill
    const billResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId: property.id,
        periodStart: '2024-03-01',
        periodEnd: '2024-03-31',
        electricityOpening: 1300,
        electricityClosing: 1450,
        electricityRate: 2.50,
        waterOpening: 550,
        waterClosing: 575,
        waterRateTier1: 10.00,
        waterRateTier2: 15.00,
        waterRateTier3: 20.00,
        sanitationOpening: 550,
        sanitationClosing: 575,
        sanitationRateTier1: 8.00,
        sanitationRateTier2: 12.00,
        sanitationRateTier3: 16.00
      }
    });

    const bill = await billResponse.json();

    // Download the PDF
    const response = await request.get(`/api/bills/${bill.id}/pdf`, {
      headers: mockAuthHeader
    });

    expect(response.status()).toBe(200);
    expect(response.headers()['content-type']).toBe('application/pdf');
  });

  test('should delete a bill', async ({ request }) => {
    // First create a property
    const propertyResponse = await request.post('/api/properties', {
      headers: mockAuthHeader,
      data: {
        name: 'Delete Bill Test Property',
        address: '333 Delete St',
        defaultElectricityRate: 2.50,
        defaultWaterRateTier1: 10.00,
        defaultWaterRateTier2: 15.00,
        defaultWaterRateTier3: 20.00,
        defaultSanitationRateTier1: 8.00,
        defaultSanitationRateTier2: 12.00,
        defaultSanitationRateTier3: 16.00
      }
    });

    const property = await propertyResponse.json();

    // Create a bill
    const billResponse = await request.post('/api/bills', {
      headers: mockAuthHeader,
      data: {
        propertyId: property.id,
        periodStart: '2024-04-01',
        periodEnd: '2024-04-30',
        electricityOpening: 1450,
        electricityClosing: 1600,
        electricityRate: 2.50,
        waterOpening: 575,
        waterClosing: 600,
        waterRateTier1: 10.00,
        waterRateTier2: 15.00,
        waterRateTier3: 20.00,
        sanitationOpening: 575,
        sanitationClosing: 600,
        sanitationRateTier1: 8.00,
        sanitationRateTier2: 12.00,
        sanitationRateTier3: 16.00
      }
    });

    const bill = await billResponse.json();

    // Delete the bill
    const deleteResponse = await request.delete(`/api/bills/${bill.id}`, {
      headers: mockAuthHeader
    });

    expect(deleteResponse.status()).toBe(204);

    // Verify it's deleted
    const getResponse = await request.get(`/api/bills/${bill.id}`, {
      headers: mockAuthHeader
    });

    expect(getResponse.status()).toBe(404);
  });
});

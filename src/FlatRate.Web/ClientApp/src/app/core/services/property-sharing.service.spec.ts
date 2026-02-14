import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { PropertySharingService } from './property-sharing.service';
import { Collaborator } from '../models/collaborator.model';

/**
 * Helper to flush microtasks so that chained async calls (e.g. loadCollaborators
 * called inside inviteCollaborator) have a chance to issue their HTTP requests.
 */
function flushMicrotasks(): Promise<void> {
  return Promise.resolve();
}

describe('PropertySharingService', () => {
  let service: PropertySharingService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PropertySharingService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(PropertySharingService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  describe('initial state', () => {
    it('should have empty collaborators', () => {
      expect(service.collaborators()).toEqual([]);
    });

    it('should have loading as false', () => {
      expect(service.loading()).toBe(false);
    });

    it('should have error as null', () => {
      expect(service.error()).toBeNull();
    });
  });

  describe('clearCollaborators', () => {
    it('should reset collaborators and error', async () => {
      // First load some data
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush([
        {
          userId: 'u1',
          email: 'test@example.com',
          name: 'Test',
          role: 'Editor',
          isPending: false,
          createdAt: '2024-01-01',
          acceptedAt: '2024-01-01',
        },
      ] as Collaborator[]);
      await loadPromise;

      expect(service.collaborators().length).toBe(1);

      // Now clear
      service.clearCollaborators();

      expect(service.collaborators()).toEqual([]);
      expect(service.error()).toBeNull();
    });
  });

  describe('clearError', () => {
    it('should reset only error', async () => {
      // Trigger an error
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
      await loadPromise;

      expect(service.error()).not.toBeNull();

      // Clear error
      service.clearError();

      expect(service.error()).toBeNull();
    });
  });

  describe('loadCollaborators', () => {
    it('should set loading to true during request', async () => {
      const promise = service.loadCollaborators('prop-1');
      expect(service.loading()).toBe(true);

      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush([]);
      await promise;
    });

    it('should set collaborators and loading to false on success', async () => {
      const mockCollaborators: Collaborator[] = [
        {
          userId: 'u1',
          email: 'test@example.com',
          name: 'Test User',
          role: 'Editor',
          isPending: false,
          createdAt: '2024-01-01T00:00:00Z',
          acceptedAt: '2024-01-01T00:00:00Z',
        },
      ];

      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush(mockCollaborators);
      await loadPromise;

      expect(service.collaborators()).toEqual(mockCollaborators);
      expect(service.loading()).toBe(false);
    });

    it('should set error message and loading to false on error', async () => {
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
      await loadPromise;

      expect(service.error()).not.toBeNull();
      expect(service.loading()).toBe(false);
    });
  });

  describe('inviteCollaborator', () => {
    it('should reload collaborators and return true on success', async () => {
      const invitePromise = service.inviteCollaborator('prop-1', {
        email: 'new@example.com',
      });

      // First: POST invite
      const postReq = httpTesting.expectOne(
        (req) => req.url === '/api/properties/prop-1/collaborators' && req.method === 'POST'
      );
      postReq.flush({});

      // Wait for microtask so loadCollaborators fires its GET
      await flushMicrotasks();

      // Second: GET collaborators (reload)
      const getReq = httpTesting.expectOne(
        (req) => req.url === '/api/properties/prop-1/collaborators' && req.method === 'GET'
      );
      getReq.flush([]);

      const result = await invitePromise;
      expect(result).toBe(true);
    });

    it('should set error message and return false on error', async () => {
      const invitePromise = service.inviteCollaborator('prop-1', {
        email: 'new@example.com',
      });

      const postReq = httpTesting.expectOne(
        (req) => req.url === '/api/properties/prop-1/collaborators' && req.method === 'POST'
      );
      postReq.flush(
        { error: 'User already has access to this property.' },
        { status: 400, statusText: 'Bad Request' }
      );

      const result = await invitePromise;
      expect(result).toBe(false);
      expect(service.error()).toBe('User already has access to this property.');
    });
  });

  describe('revokeAccess', () => {
    it('should reload collaborators and return true on success', async () => {
      const revokePromise = service.revokeAccess('prop-1', 'user-1');

      // First: DELETE
      const deleteReq = httpTesting.expectOne(
        (req) =>
          req.url === '/api/properties/prop-1/collaborators/user-1' && req.method === 'DELETE'
      );
      deleteReq.flush({});

      // Wait for microtask so loadCollaborators fires its GET
      await flushMicrotasks();

      // Second: GET collaborators (reload)
      const getReq = httpTesting.expectOne(
        (req) => req.url === '/api/properties/prop-1/collaborators' && req.method === 'GET'
      );
      getReq.flush([]);

      const result = await revokePromise;
      expect(result).toBe(true);
    });

    it('should set error message and return false on error', async () => {
      const revokePromise = service.revokeAccess('prop-1', 'user-1');

      const deleteReq = httpTesting.expectOne(
        (req) =>
          req.url === '/api/properties/prop-1/collaborators/user-1' && req.method === 'DELETE'
      );
      deleteReq.flush(
        { error: 'Only the property owner can revoke access.' },
        { status: 400, statusText: 'Bad Request' }
      );

      const result = await revokePromise;
      expect(result).toBe(false);
      expect(service.error()).toBe('Only the property owner can revoke access.');
    });
  });

  describe('getErrorMessage (via loadCollaborators)', () => {
    it('should return auth message for 401', async () => {
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
      await loadPromise;

      expect(service.error()).toBe('Please log in to continue.');
    });

    it('should return connection message for network error (status 0)', async () => {
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.error(new ProgressEvent('error'), { status: 0, statusText: 'Unknown Error' });
      await loadPromise;

      expect(service.error()).toBe('Unable to connect to server. Please check your connection.');
    });

    it('should extract error message from API error body', async () => {
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush(
        { error: 'Property not found.' },
        { status: 404, statusText: 'Not Found' }
      );
      await loadPromise;

      expect(service.error()).toBe('Property not found.');
    });

    it('should fall back to HttpErrorResponse message for server error without error body', async () => {
      const loadPromise = service.loadCollaborators('prop-1');
      const req = httpTesting.expectOne('/api/properties/prop-1/collaborators');
      req.flush(null, { status: 500, statusText: 'Internal Server Error' });
      await loadPromise;

      expect(service.error()).toContain('500 Internal Server Error');
    });
  });
});

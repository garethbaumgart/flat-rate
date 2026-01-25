import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { Collaborator, InviteRequest } from '../models/collaborator.model';

@Injectable({ providedIn: 'root' })
export class PropertySharingService {
  private readonly http = inject(HttpClient);

  // State signals
  private readonly _collaborators = signal<Collaborator[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public readonly signals
  readonly collaborators = this._collaborators.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  private getApiUrl(propertyId: string): string {
    return `/api/properties/${propertyId}/collaborators`;
  }

  async loadCollaborators(propertyId: string): Promise<void> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const collaborators = await firstValueFrom(
        this.http.get<Collaborator[]>(this.getApiUrl(propertyId))
      );
      this._collaborators.set(collaborators);
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
    } finally {
      this._loading.set(false);
    }
  }

  async inviteCollaborator(propertyId: string, request: InviteRequest): Promise<boolean> {
    this._loading.set(true);
    this._error.set(null);

    try {
      await firstValueFrom(
        this.http.post(this.getApiUrl(propertyId), request)
      );
      await this.loadCollaborators(propertyId);
      return true;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return false;
    } finally {
      this._loading.set(false);
    }
  }

  async revokeAccess(propertyId: string, userId: string): Promise<boolean> {
    this._loading.set(true);
    this._error.set(null);

    try {
      await firstValueFrom(
        this.http.delete(`${this.getApiUrl(propertyId)}/${userId}`)
      );
      await this.loadCollaborators(propertyId);
      return true;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return false;
    } finally {
      this._loading.set(false);
    }
  }

  clearCollaborators(): void {
    this._collaborators.set([]);
    this._error.set(null);
  }

  clearError(): void {
    this._error.set(null);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) {
        return 'Please log in to continue.';
      }

      if (error.status === 0) {
        return 'Unable to connect to server. Please check your connection.';
      }

      // Handle API error responses with { error: "message" } format
      if (error.error?.error && typeof error.error.error === 'string') {
        return error.error.error;
      }

      return error.message || 'An unexpected error occurred.';
    }

    return 'An unexpected error occurred.';
  }
}

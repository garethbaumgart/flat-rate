import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  Property,
  CreatePropertyRequest,
  UpdatePropertyRequest,
  SetPropertyRatesRequest
} from '../models/property.model';

@Injectable({ providedIn: 'root' })
export class PropertyService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/properties';

  // State signals
  private readonly _properties = signal<Property[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _selectedProperty = signal<Property | null>(null);

  // Public readonly signals
  readonly properties = this._properties.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly selectedProperty = this._selectedProperty.asReadonly();

  // Computed signals
  readonly hasProperties = computed(() => this._properties().length > 0);
  readonly propertyCount = computed(() => this._properties().length);

  async loadProperties(): Promise<void> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const properties = await firstValueFrom(
        this.http.get<Property[]>(this.apiUrl)
      );
      this._properties.set(properties);
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
    } finally {
      this._loading.set(false);
    }
  }

  async getPropertyById(id: string): Promise<Property | null> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const property = await firstValueFrom(
        this.http.get<Property>(`${this.apiUrl}/${id}`)
      );
      this._selectedProperty.set(property);
      return property;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return null;
    } finally {
      this._loading.set(false);
    }
  }

  async createProperty(request: CreatePropertyRequest): Promise<string | null> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const response = await firstValueFrom(
        this.http.post<{ id: string }>(this.apiUrl, request)
      );
      await this.loadProperties();
      return response.id;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return null;
    } finally {
      this._loading.set(false);
    }
  }

  async updateProperty(id: string, request: UpdatePropertyRequest): Promise<boolean> {
    this._loading.set(true);
    this._error.set(null);

    try {
      await firstValueFrom(
        this.http.put(`${this.apiUrl}/${id}`, request)
      );
      await this.loadProperties();
      return true;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return false;
    } finally {
      this._loading.set(false);
    }
  }

  async setPropertyRates(id: string, request: SetPropertyRatesRequest): Promise<boolean> {
    this._loading.set(true);
    this._error.set(null);

    try {
      await firstValueFrom(
        this.http.put(`${this.apiUrl}/${id}/rates`, request)
      );
      await this.loadProperties();
      return true;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return false;
    } finally {
      this._loading.set(false);
    }
  }

  async deleteProperty(id: string): Promise<boolean> {
    this._loading.set(true);
    this._error.set(null);

    try {
      await firstValueFrom(
        this.http.delete(`${this.apiUrl}/${id}`)
      );
      await this.loadProperties();
      return true;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return false;
    } finally {
      this._loading.set(false);
    }
  }

  selectProperty(property: Property | null): void {
    this._selectedProperty.set(property);
  }

  clearError(): void {
    this._error.set(null);
  }

  private getErrorMessage(error: unknown): string {
    if (error && typeof error === 'object') {
      const err = error as { error?: { error?: string }; message?: string; status?: number };

      if (err.status === 401) {
        return 'Please log in to continue.';
      }

      if (err.error?.error) {
        return err.error.error;
      }

      if (err.message) {
        return err.message;
      }
    }

    return 'An unexpected error occurred.';
  }
}

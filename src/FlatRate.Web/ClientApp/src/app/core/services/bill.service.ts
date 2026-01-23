import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { Bill, CreateBillRequest, BillPreview } from '../models/bill.model';

/** VAT rate in South Africa (15%). */
const VAT_RATE = 0.15;

/** Tier boundaries for water/sanitation (in kL). */
const TIER_1_LIMIT = 6;
const TIER_2_LIMIT = 15;

@Injectable({ providedIn: 'root' })
export class BillService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/bills';

  // State signals
  private readonly _bills = signal<Bill[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _selectedBill = signal<Bill | null>(null);

  // Public readonly signals
  readonly bills = this._bills.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly selectedBill = this._selectedBill.asReadonly();

  // Computed signals
  readonly hasBills = computed(() => this._bills().length > 0);
  readonly billCount = computed(() => this._bills().length);

  async loadBills(propertyId?: string): Promise<void> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const url = propertyId ? `${this.apiUrl}?propertyId=${propertyId}` : this.apiUrl;
      const bills = await firstValueFrom(
        this.http.get<Bill[]>(url)
      );
      this._bills.set(bills);
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
    } finally {
      this._loading.set(false);
    }
  }

  async getBillById(id: string): Promise<Bill | null> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const bill = await firstValueFrom(
        this.http.get<Bill>(`${this.apiUrl}/${id}`)
      );
      this._selectedBill.set(bill);
      return bill;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return null;
    } finally {
      this._loading.set(false);
    }
  }

  async createBill(request: CreateBillRequest): Promise<string | null> {
    this._loading.set(true);
    this._error.set(null);

    try {
      const response = await firstValueFrom(
        this.http.post<{ id: string }>(this.apiUrl, request)
      );
      await this.loadBills();
      return response.id;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return null;
    } finally {
      this._loading.set(false);
    }
  }

  async deleteBill(id: string): Promise<boolean> {
    this._loading.set(true);
    this._error.set(null);

    try {
      await firstValueFrom(
        this.http.delete(`${this.apiUrl}/${id}`)
      );
      await this.loadBills();
      return true;
    } catch (err) {
      this._error.set(this.getErrorMessage(err));
      return false;
    } finally {
      this._loading.set(false);
    }
  }

  selectBill(bill: Bill | null): void {
    this._selectedBill.set(bill);
  }

  clearError(): void {
    this._error.set(null);
  }

  /**
   * Download invoice PDF for a bill.
   * Returns a Blob that can be used to create a download link.
   */
  async getInvoicePdfBlob(id: string): Promise<Blob> {
    return firstValueFrom(
      this.http.get(`${this.apiUrl}/${id}/pdf`, { responseType: 'blob' as const })
    );
  }

  /**
   * Calculate a live preview of the bill based on meter readings and rates.
   * This mirrors the backend BillingCalculator logic for instant feedback.
   */
  calculatePreview(
    electricityOpening: number,
    electricityClosing: number,
    waterOpening: number,
    waterClosing: number,
    sanitationOpening: number,
    sanitationClosing: number,
    electricityRate: number,
    waterRateTier1: number,
    waterRateTier2: number,
    waterRateTier3: number,
    sanitationRateTier1: number,
    sanitationRateTier2: number,
    sanitationRateTier3: number
  ): BillPreview {
    // Calculate units used
    const electricityUnits = Math.max(0, electricityClosing - electricityOpening);
    const waterUnits = Math.max(0, waterClosing - waterOpening);
    const sanitationUnits = Math.max(0, sanitationClosing - sanitationOpening);

    // Calculate costs
    const electricityCost = this.calculateFlatRateCost(electricityUnits, electricityRate);
    const waterCost = this.calculateTieredCost(waterUnits, waterRateTier1, waterRateTier2, waterRateTier3);
    const sanitationCost = this.calculateTieredCost(sanitationUnits, sanitationRateTier1, sanitationRateTier2, sanitationRateTier3);

    // Calculate totals
    const subtotal = electricityCost + waterCost + sanitationCost;
    const vatAmount = subtotal * VAT_RATE;
    const total = subtotal + vatAmount;

    return {
      electricityUnits,
      waterUnits,
      sanitationUnits,
      electricityCost,
      waterCost,
      sanitationCost,
      subtotal,
      vatAmount,
      total
    };
  }

  /**
   * Calculate cost using flat rate (for electricity).
   */
  private calculateFlatRateCost(units: number, rate: number): number {
    if (units < 0 || rate < 0) return 0;
    return units * rate;
  }

  /**
   * Calculate cost using tiered pricing (for water/sanitation).
   * Tier 1: 0-6 kL
   * Tier 2: 7-15 kL (next 9 kL)
   * Tier 3: 16+ kL (remaining)
   */
  private calculateTieredCost(
    units: number,
    tier1Rate: number,
    tier2Rate: number,
    tier3Rate: number
  ): number {
    if (units < 0) return 0;

    // Clamp rates to non-negative values to avoid negative invoices
    const safeTier1Rate = Math.max(0, tier1Rate || 0);
    const safeTier2Rate = Math.max(0, tier2Rate || 0);
    const safeTier3Rate = Math.max(0, tier3Rate || 0);

    let remaining = units;
    let cost = 0;

    // Tier 1: 0-6 kL
    const tier1Units = Math.min(remaining, TIER_1_LIMIT);
    cost += tier1Units * safeTier1Rate;
    remaining -= tier1Units;

    if (remaining <= 0) return cost;

    // Tier 2: 7-15 kL (9 more kL)
    const tier2Capacity = TIER_2_LIMIT - TIER_1_LIMIT;
    const tier2Units = Math.min(remaining, tier2Capacity);
    cost += tier2Units * safeTier2Rate;
    remaining -= tier2Units;

    if (remaining <= 0) return cost;

    // Tier 3: 16+ kL (remaining)
    cost += remaining * safeTier3Rate;

    return cost;
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

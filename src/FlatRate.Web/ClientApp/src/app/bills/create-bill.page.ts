import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { Select } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePicker } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';
import { PropertyService } from '../core/services/property.service';
import { BillService } from '../core/services/bill.service';
import { Property } from '../core/models/property.model';
import { CreateBillRequest, BillPreview } from '../core/models/bill.model';

@Component({
  selector: 'app-create-bill',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    Select,
    InputNumberModule,
    DatePicker,
    ToastModule,
    DividerModule
  ],
  providers: [MessageService],
  template: `
    <div class="p-4 md:p-8 max-w-4xl mx-auto">
      <div class="mb-6">
        <h1 class="text-2xl md:text-3xl font-bold text-gray-900">Create Bill</h1>
        <p class="text-gray-600 mt-1">Generate a new utility bill</p>
      </div>

      <form (ngSubmit)="onSubmit()" class="flex flex-col gap-6">
        <!-- Property Selection -->
        <p-card header="Property">
          <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-2">
              <label for="property" class="font-medium">Select Property *</label>
              <p-select
                id="property"
                [options]="propertyOptions()"
                [(ngModel)]="selectedPropertyId"
                name="property"
                optionLabel="label"
                optionValue="value"
                placeholder="Select a property"
                styleClass="w-full"
                (onChange)="onPropertyChange($event.value)"
              />
            </div>
            @if (selectedProperty()) {
              <div class="text-sm text-gray-600 bg-gray-50 p-3 rounded">
                <strong>{{ selectedProperty()?.name }}</strong><br>
                {{ selectedProperty()?.address }}
              </div>
            }
          </div>
        </p-card>

        <!-- Billing Period -->
        <p-card header="Billing Period">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label for="periodStart" class="font-medium">Period Start *</label>
              <p-datepicker
                id="periodStart"
                [(ngModel)]="periodStart"
                name="periodStart"
                dateFormat="yy-mm-dd"
                styleClass="w-full"
                [showIcon]="true"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label for="periodEnd" class="font-medium">Period End *</label>
              <p-datepicker
                id="periodEnd"
                [(ngModel)]="periodEnd"
                name="periodEnd"
                dateFormat="yy-mm-dd"
                styleClass="w-full"
                [showIcon]="true"
              />
            </div>
          </div>
        </p-card>

        <!-- Meter Readings -->
        <p-card header="Meter Readings">
          <div class="flex flex-col gap-6">
            <!-- Electricity -->
            <div>
              <h4 class="font-medium text-lg mb-3">Electricity (kWh)</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="elecOpening" class="text-sm">Opening Reading</label>
                  <p-inputNumber
                    id="elecOpening"
                    [(ngModel)]="electricityOpening"
                    name="elecOpening"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (onInput)="updatePreview()"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="elecClosing" class="text-sm">Closing Reading</label>
                  <p-inputNumber
                    id="elecClosing"
                    [(ngModel)]="electricityClosing"
                    name="elecClosing"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (onInput)="updatePreview()"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-sm">Units Used</label>
                  <div class="p-3 bg-gray-100 rounded font-medium">
                    {{ preview().electricityUnits | number:'1.2-2' }} kWh
                  </div>
                </div>
              </div>
            </div>

            <p-divider />

            <!-- Water -->
            <div>
              <h4 class="font-medium text-lg mb-3">Water (kL)</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="waterOpening" class="text-sm">Opening Reading</label>
                  <p-inputNumber
                    id="waterOpening"
                    [(ngModel)]="waterOpening"
                    name="waterOpening"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (onInput)="updatePreview()"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="waterClosing" class="text-sm">Closing Reading</label>
                  <p-inputNumber
                    id="waterClosing"
                    [(ngModel)]="waterClosing"
                    name="waterClosing"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (onInput)="updatePreview()"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-sm">Units Used</label>
                  <div class="p-3 bg-gray-100 rounded font-medium">
                    {{ preview().waterUnits | number:'1.2-2' }} kL
                  </div>
                </div>
              </div>
            </div>

            <p-divider />

            <!-- Sanitation -->
            <div>
              <h4 class="font-medium text-lg mb-3">Sanitation (kL)</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="sanitationOpening" class="text-sm">Opening Reading</label>
                  <p-inputNumber
                    id="sanitationOpening"
                    [(ngModel)]="sanitationOpening"
                    name="sanitationOpening"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (onInput)="updatePreview()"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="sanitationClosing" class="text-sm">Closing Reading</label>
                  <p-inputNumber
                    id="sanitationClosing"
                    [(ngModel)]="sanitationClosing"
                    name="sanitationClosing"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (onInput)="updatePreview()"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-sm">Units Used</label>
                  <div class="p-3 bg-gray-100 rounded font-medium">
                    {{ preview().sanitationUnits | number:'1.2-2' }} kL
                  </div>
                </div>
              </div>
            </div>
          </div>
        </p-card>

        <!-- Tariff Rates -->
        <p-card header="Tariff Rates">
          <p class="text-sm text-gray-600 mb-4">
            @if (selectedProperty()?.defaultElectricityRate !== null) {
              Rates pre-filled from property defaults. You can adjust them for this bill.
            } @else {
              Enter the tariff rates for this billing period.
            }
          </p>
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <!-- Electricity Rate -->
            <div class="flex flex-col gap-2">
              <label for="elecRate" class="font-medium">Electricity (per kWh)</label>
              <p-inputNumber
                id="elecRate"
                [(ngModel)]="electricityRate"
                name="elecRate"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>

            <!-- Water Rates -->
            <div class="flex flex-col gap-2">
              <label for="waterTier1" class="font-medium">Water Tier 1 (0-6 kL)</label>
              <p-inputNumber
                id="waterTier1"
                [(ngModel)]="waterRateTier1"
                name="waterTier1"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label for="waterTier2" class="font-medium">Water Tier 2 (7-15 kL)</label>
              <p-inputNumber
                id="waterTier2"
                [(ngModel)]="waterRateTier2"
                name="waterTier2"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label for="waterTier3" class="font-medium">Water Tier 3 (16+ kL)</label>
              <p-inputNumber
                id="waterTier3"
                [(ngModel)]="waterRateTier3"
                name="waterTier3"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>

            <!-- Sanitation Rates -->
            <div class="flex flex-col gap-2">
              <label for="sanitationTier1" class="font-medium">Sanitation Tier 1 (0-6 kL)</label>
              <p-inputNumber
                id="sanitationTier1"
                [(ngModel)]="sanitationRateTier1"
                name="sanitationTier1"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label for="sanitationTier2" class="font-medium">Sanitation Tier 2 (7-15 kL)</label>
              <p-inputNumber
                id="sanitationTier2"
                [(ngModel)]="sanitationRateTier2"
                name="sanitationTier2"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label for="sanitationTier3" class="font-medium">Sanitation Tier 3 (16+ kL)</label>
              <p-inputNumber
                id="sanitationTier3"
                [(ngModel)]="sanitationRateTier3"
                name="sanitationTier3"
                [minFractionDigits]="2"
                [maxFractionDigits]="4"
                prefix="R "
                styleClass="w-full"
                (onInput)="updatePreview()"
              />
            </div>
          </div>
        </p-card>

        <!-- Cost Preview -->
        <p-card header="Cost Preview" styleClass="bg-primary-50">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <div class="flex justify-between py-2">
                <span class="text-gray-600">Electricity:</span>
                <span class="font-medium">R {{ preview().electricityCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between py-2">
                <span class="text-gray-600">Water:</span>
                <span class="font-medium">R {{ preview().waterCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between py-2">
                <span class="text-gray-600">Sanitation:</span>
                <span class="font-medium">R {{ preview().sanitationCost | number:'1.2-2' }}</span>
              </div>
            </div>
            <div>
              <div class="flex justify-between py-2">
                <span class="text-gray-600">Subtotal:</span>
                <span class="font-medium">R {{ preview().subtotal | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between py-2">
                <span class="text-gray-600">VAT (15%):</span>
                <span class="font-medium">R {{ preview().vatAmount | number:'1.2-2' }}</span>
              </div>
              <p-divider />
              <div class="flex justify-between py-2 text-lg">
                <span class="font-bold">Total:</span>
                <span class="font-bold text-primary">R {{ preview().total | number:'1.2-2' }}</span>
              </div>
            </div>
          </div>
        </p-card>

        <!-- Actions -->
        <div class="flex justify-end gap-3">
          <p-button
            label="Cancel"
            icon="pi pi-times"
            severity="secondary"
            [text]="true"
            (onClick)="onCancel()"
          />
          <p-button
            label="Create Bill"
            icon="pi pi-check"
            [loading]="billService.loading()"
            [disabled]="!isFormValid()"
            type="submit"
          />
        </div>
      </form>
    </div>

    <p-toast />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateBillPage implements OnInit {
  readonly propertyService = inject(PropertyService);
  readonly billService = inject(BillService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  // Property selection
  selectedPropertyId: string | null = null;
  selectedProperty = signal<Property | null>(null);

  // Billing period
  periodStart: Date | null = null;
  periodEnd: Date | null = null;

  // Meter readings
  electricityOpening: number = 0;
  electricityClosing: number = 0;
  waterOpening: number = 0;
  waterClosing: number = 0;
  sanitationOpening: number = 0;
  sanitationClosing: number = 0;

  // Tariff rates
  electricityRate: number = 0;
  waterRateTier1: number = 0;
  waterRateTier2: number = 0;
  waterRateTier3: number = 0;
  sanitationRateTier1: number = 0;
  sanitationRateTier2: number = 0;
  sanitationRateTier3: number = 0;

  // Computed preview
  preview = signal<BillPreview>({
    electricityUnits: 0,
    waterUnits: 0,
    sanitationUnits: 0,
    electricityCost: 0,
    waterCost: 0,
    sanitationCost: 0,
    subtotal: 0,
    vatAmount: 0,
    total: 0
  });

  // Property dropdown options
  propertyOptions = computed(() =>
    this.propertyService.properties().map(p => ({
      label: `${p.name} - ${p.address}`,
      value: p.id
    }))
  );

  ngOnInit(): void {
    this.propertyService.loadProperties();

    // Set default billing period (previous month)
    const today = new Date();
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 1);
    const lastMonthEnd = new Date(today.getFullYear(), today.getMonth(), 0);
    this.periodStart = lastMonth;
    this.periodEnd = lastMonthEnd;
  }

  onPropertyChange(propertyId: string): void {
    const property = this.propertyService.properties().find(p => p.id === propertyId);
    this.selectedProperty.set(property || null);

    // Pre-fill rates from property defaults
    if (property) {
      this.electricityRate = property.defaultElectricityRate ?? 0;
      this.waterRateTier1 = property.defaultWaterRateTier1 ?? 0;
      this.waterRateTier2 = property.defaultWaterRateTier2 ?? 0;
      this.waterRateTier3 = property.defaultWaterRateTier3 ?? 0;
      this.sanitationRateTier1 = property.defaultSanitationRateTier1 ?? 0;
      this.sanitationRateTier2 = property.defaultSanitationRateTier2 ?? 0;
      this.sanitationRateTier3 = property.defaultSanitationRateTier3 ?? 0;

      this.updatePreview();
    }
  }

  updatePreview(): void {
    const newPreview = this.billService.calculatePreview(
      this.electricityOpening || 0,
      this.electricityClosing || 0,
      this.waterOpening || 0,
      this.waterClosing || 0,
      this.sanitationOpening || 0,
      this.sanitationClosing || 0,
      this.electricityRate || 0,
      this.waterRateTier1 || 0,
      this.waterRateTier2 || 0,
      this.waterRateTier3 || 0,
      this.sanitationRateTier1 || 0,
      this.sanitationRateTier2 || 0,
      this.sanitationRateTier3 || 0
    );
    this.preview.set(newPreview);
  }

  isFormValid(): boolean {
    return !!(
      this.selectedPropertyId &&
      this.periodStart &&
      this.periodEnd &&
      this.periodEnd >= this.periodStart &&
      this.electricityClosing >= this.electricityOpening &&
      this.waterClosing >= this.waterOpening &&
      this.sanitationClosing >= this.sanitationOpening &&
      this.electricityRate > 0
    );
  }

  async onSubmit(): Promise<void> {
    if (!this.isFormValid()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields correctly.'
      });
      return;
    }

    const request: CreateBillRequest = {
      propertyId: this.selectedPropertyId!,
      periodStart: this.formatDate(this.periodStart!),
      periodEnd: this.formatDate(this.periodEnd!),
      electricityReadingOpening: this.electricityOpening,
      electricityReadingClosing: this.electricityClosing,
      waterReadingOpening: this.waterOpening,
      waterReadingClosing: this.waterClosing,
      sanitationReadingOpening: this.sanitationOpening,
      sanitationReadingClosing: this.sanitationClosing,
      electricityRate: this.electricityRate,
      waterRateTier1: this.waterRateTier1,
      waterRateTier2: this.waterRateTier2,
      waterRateTier3: this.waterRateTier3,
      sanitationRateTier1: this.sanitationRateTier1,
      sanitationRateTier2: this.sanitationRateTier2,
      sanitationRateTier3: this.sanitationRateTier3
    };

    const billId = await this.billService.createBill(request);

    if (billId) {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Bill created successfully.'
      });
      // Navigate to bills list after a short delay
      setTimeout(() => this.router.navigate(['/bills']), 1500);
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: this.billService.error() || 'Failed to create bill.'
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/bills']);
  }

  private formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}

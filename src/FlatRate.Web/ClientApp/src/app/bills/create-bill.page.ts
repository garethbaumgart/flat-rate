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
import { Tooltip } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { PropertyService } from '../core/services/property.service';
import { BillService } from '../core/services/bill.service';
import { Property } from '../core/models/property.model';
import { CreateBillRequest } from '../core/models/bill.model';
import { formatDateToISO } from '../core/utils/date-utils';

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
    DividerModule,
    Tooltip
  ],
  providers: [MessageService],
  template: `
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <h1 class="text-2xl md:text-3xl font-bold" style="color: var(--color-text-primary);">Create Bill</h1>
        <p style="color: var(--color-text-secondary);" class="mt-1">Generate a new utility bill</p>
      </div>

      <form (ngSubmit)="onSubmit()" class="flex flex-col gap-6">
        <!-- Property Selection -->
        <p-card header="Property">
          <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-2">
              <label for="property" class="font-medium" style="color: var(--color-text-primary);">Select Property *</label>
              <p-select
                id="property"
                [options]="propertyOptions()"
                [ngModel]="selectedPropertyId()"
                (ngModelChange)="onPropertyChange($event)"
                name="property"
                optionLabel="label"
                optionValue="value"
                placeholder="Select a property"
                styleClass="w-full"
              />
            </div>
            @if (selectedProperty()) {
              <div class="text-sm p-3 rounded-lg" style="background: var(--color-bg-tertiary); color: var(--color-text-secondary);">
                <strong style="color: var(--color-text-primary);">{{ selectedProperty()?.name }}</strong><br>
                {{ selectedProperty()?.address }}
              </div>
            }
          </div>
        </p-card>

        <!-- Billing Period -->
        <p-card header="Billing Period">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label for="periodStart" class="font-medium" style="color: var(--color-text-primary);">Period Start *</label>
              <p-datepicker
                id="periodStart"
                [ngModel]="periodStart()"
                (ngModelChange)="periodStart.set($event)"
                name="periodStart"
                dateFormat="yy-mm-dd"
                styleClass="w-full"
                [showIcon]="true"
              />
            </div>
            <div class="flex flex-col gap-2">
              <label for="periodEnd" class="font-medium" style="color: var(--color-text-primary);">Period End *</label>
              <p-datepicker
                id="periodEnd"
                [ngModel]="periodEnd()"
                (ngModelChange)="periodEnd.set($event)"
                name="periodEnd"
                dateFormat="yy-mm-dd"
                styleClass="w-full"
                [showIcon]="true"
              />
            </div>
          </div>
        </p-card>

        <!-- Meter Readings -->
        <p-card>
          <ng-template pTemplate="title">
            <div class="flex items-center gap-2">
              <span>Meter Readings</span>
              <i class="pi pi-info-circle text-sm" style="color: var(--color-text-muted);"
                pTooltip="Enter the opening and closing readings from the utility meter. Units consumed are calculated automatically."
                tooltipPosition="top"></i>
            </div>
          </ng-template>
          <div class="flex flex-col gap-6">
            <!-- Electricity -->
            <div>
              <h4 class="font-medium text-lg mb-3" style="color: var(--color-text-primary);">Electricity (kWh)</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="elecOpening" class="text-sm">Opening Reading</label>
                  <p-inputNumber
                    id="elecOpening"
                    [ngModel]="electricityOpening()"
                    (ngModelChange)="electricityOpening.set($event ?? 0)"
                    name="elecOpening"
                    mode="decimal"
                    [useGrouping]="false"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="elecClosing" class="text-sm">Closing Reading</label>
                  <p-inputNumber
                    id="elecClosing"
                    [ngModel]="electricityClosing()"
                    (ngModelChange)="electricityClosing.set($event ?? 0)"
                    name="elecClosing"
                    mode="decimal"
                    [useGrouping]="false"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-sm" style="color: var(--color-text-secondary);">Units Used</label>
                  <div class="p-3 rounded font-medium" style="background: var(--color-bg-tertiary); color: var(--color-text-primary);">
                    {{ preview().electricityUnits | number:'1.2-2' }} kWh
                  </div>
                </div>
              </div>
            </div>

            <p-divider />

            <!-- Water -->
            <div>
              <h4 class="font-medium text-lg mb-3" style="color: var(--color-text-primary);">Water (kL)</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="waterOpening" class="text-sm">Opening Reading</label>
                  <p-inputNumber
                    id="waterOpening"
                    [ngModel]="waterOpening()"
                    (ngModelChange)="waterOpening.set($event ?? 0)"
                    name="waterOpening"
                    mode="decimal"
                    [useGrouping]="false"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="waterClosing" class="text-sm">Closing Reading</label>
                  <p-inputNumber
                    id="waterClosing"
                    [ngModel]="waterClosing()"
                    (ngModelChange)="waterClosing.set($event ?? 0)"
                    name="waterClosing"
                    mode="decimal"
                    [useGrouping]="false"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-sm" style="color: var(--color-text-secondary);">Units Used</label>
                  <div class="p-3 rounded font-medium" style="background: var(--color-bg-tertiary); color: var(--color-text-primary);">
                    {{ preview().waterUnits | number:'1.2-2' }} kL
                  </div>
                </div>
              </div>
            </div>

            <p-divider />

            <!-- Sanitation -->
            <div>
              <h4 class="font-medium text-lg mb-3" style="color: var(--color-text-primary);">Sanitation (kL)</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="sanitationOpening" class="text-sm">Opening Reading</label>
                  <p-inputNumber
                    id="sanitationOpening"
                    [ngModel]="sanitationOpening()"
                    (ngModelChange)="sanitationOpening.set($event ?? 0)"
                    name="sanitationOpening"
                    mode="decimal"
                    [useGrouping]="false"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="sanitationClosing" class="text-sm">Closing Reading</label>
                  <p-inputNumber
                    id="sanitationClosing"
                    [ngModel]="sanitationClosing()"
                    (ngModelChange)="sanitationClosing.set($event ?? 0)"
                    name="sanitationClosing"
                    mode="decimal"
                    [useGrouping]="false"
                    [minFractionDigits]="0"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-sm" style="color: var(--color-text-secondary);">Units Used</label>
                  <div class="p-3 rounded font-medium" style="background: var(--color-bg-tertiary); color: var(--color-text-primary);">
                    {{ preview().sanitationUnits | number:'1.2-2' }} kL
                  </div>
                </div>
              </div>
            </div>
          </div>
        </p-card>

        <!-- Tariff Rates -->
        <p-card>
          <ng-template pTemplate="title">
            <div class="flex items-center gap-2">
              <span>Tariff Rates</span>
              <i class="pi pi-info-circle text-sm" style="color: var(--color-text-muted);"
                pTooltip="Rates are pre-filled from your property defaults. You can adjust them for this specific bill."
                tooltipPosition="top"></i>
            </div>
          </ng-template>
          <p class="text-sm mb-4" style="color: var(--color-text-secondary);">
            @if (selectedProperty()?.defaultElectricityRate !== null) {
              Rates pre-filled from property defaults. You can adjust them for this bill.
            } @else {
              Enter the tariff rates for this billing period.
            }
          </p>
          <div class="flex flex-col gap-4">
            <!-- Electricity -->
            <div class="relative rounded-[10px] border-[1.5px] p-4 pt-5" style="border-color: var(--color-border);"
                 role="group" aria-labelledby="bill-electricity-legend">
              <div id="bill-electricity-legend" class="absolute -top-2.5 left-4 px-2 text-xs font-semibold uppercase tracking-wide"
                   style="background: var(--color-bg-card); color: var(--color-warning);">
                <i class="pi pi-bolt"></i> Electricity
              </div>
              <div class="grid grid-cols-1 gap-4 md:max-w-[33%]">
                <div class="flex flex-col gap-2">
                  <label for="elecRate" class="font-medium">Rate (per kWh)</label>
                  <p-inputNumber
                    id="elecRate"
                    [ngModel]="electricityRate()"
                    (ngModelChange)="electricityRate.set($event ?? 0)"
                    name="elecRate"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
              </div>
            </div>

            <!-- Water -->
            <div class="relative rounded-[10px] border-[1.5px] p-4 pt-5" style="border-color: var(--color-border);"
                 role="group" aria-labelledby="bill-water-legend">
              <div id="bill-water-legend" class="absolute -top-2.5 left-4 px-2 text-xs font-semibold uppercase tracking-wide"
                   style="background: var(--color-bg-card); color: var(--color-info);">
                <i class="pi pi-wave-pulse"></i> Water
              </div>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="waterTier1" class="font-medium">Tier 1 (0–6 kL)</label>
                  <p-inputNumber
                    id="waterTier1"
                    [ngModel]="waterRateTier1()"
                    (ngModelChange)="waterRateTier1.set($event ?? 0)"
                    name="waterTier1"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="waterTier2" class="font-medium">Tier 2 (7–15 kL)</label>
                  <p-inputNumber
                    id="waterTier2"
                    [ngModel]="waterRateTier2()"
                    (ngModelChange)="waterRateTier2.set($event ?? 0)"
                    name="waterTier2"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="waterTier3" class="font-medium">Tier 3 (16+ kL)</label>
                  <p-inputNumber
                    id="waterTier3"
                    [ngModel]="waterRateTier3()"
                    (ngModelChange)="waterRateTier3.set($event ?? 0)"
                    name="waterTier3"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
              </div>
            </div>

            <!-- Sanitation -->
            <div class="relative rounded-[10px] border-[1.5px] p-4 pt-5" style="border-color: var(--color-border);"
                 role="group" aria-labelledby="bill-sanitation-legend">
              <div id="bill-sanitation-legend" class="absolute -top-2.5 left-4 px-2 text-xs font-semibold uppercase tracking-wide"
                   style="background: var(--color-bg-card); color: var(--color-success);">
                <i class="pi pi-sync"></i> Sanitation
              </div>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="flex flex-col gap-2">
                  <label for="sanitationTier1" class="font-medium">Tier 1 (0–6 kL)</label>
                  <p-inputNumber
                    id="sanitationTier1"
                    [ngModel]="sanitationRateTier1()"
                    (ngModelChange)="sanitationRateTier1.set($event ?? 0)"
                    name="sanitationTier1"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="sanitationTier2" class="font-medium">Tier 2 (7–15 kL)</label>
                  <p-inputNumber
                    id="sanitationTier2"
                    [ngModel]="sanitationRateTier2()"
                    (ngModelChange)="sanitationRateTier2.set($event ?? 0)"
                    name="sanitationTier2"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
                <div class="flex flex-col gap-2">
                  <label for="sanitationTier3" class="font-medium">Tier 3 (16+ kL)</label>
                  <p-inputNumber
                    id="sanitationTier3"
                    [ngModel]="sanitationRateTier3()"
                    (ngModelChange)="sanitationRateTier3.set($event ?? 0)"
                    name="sanitationTier3"
                    mode="decimal"
                    [useGrouping]="false"
                    [min]="0"
                    [minFractionDigits]="2"
                    [maxFractionDigits]="4"
                    prefix="R "
                    styleClass="w-full"
                  />
                </div>
              </div>
            </div>
          </div>
        </p-card>

        <!-- Cost Preview -->
        <div class="rounded-xl border p-6" style="background: var(--color-accent-bg); border-color: var(--color-border);">
          <h3 class="text-lg font-semibold mb-4" style="color: var(--color-text-primary);">Cost Preview</h3>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <div class="flex justify-between py-2">
                <span style="color: var(--color-text-secondary);">Electricity:</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ preview().electricityCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between py-2">
                <span style="color: var(--color-text-secondary);">Water:</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ preview().waterCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between py-2">
                <span style="color: var(--color-text-secondary);">Sanitation:</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ preview().sanitationCost | number:'1.2-2' }}</span>
              </div>
            </div>
            <div>
              <div class="flex justify-between py-2">
                <span style="color: var(--color-text-secondary);">Subtotal:</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ preview().subtotal | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between py-2">
                <span style="color: var(--color-text-secondary);">
                  VAT (15%):
                  <i class="pi pi-info-circle text-xs ml-1" style="color: var(--color-text-muted);"
                    pTooltip="VAT is calculated at 15% on the subtotal (South African standard rate)."
                    tooltipPosition="top"></i>
                </span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ preview().vatAmount | number:'1.2-2' }}</span>
              </div>
              <p-divider />
              <div class="flex justify-between py-2 text-lg">
                <span class="font-bold" style="color: var(--color-text-primary);">Total:</span>
                <span class="font-bold" style="color: var(--color-accent);">R {{ preview().total | number:'1.2-2' }}</span>
              </div>
            </div>
          </div>
        </div>

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
  readonly selectedPropertyId = signal<string | null>(null);
  readonly selectedProperty = signal<Property | null>(null);

  // Billing period
  readonly periodStart = signal<Date | null>(null);
  readonly periodEnd = signal<Date | null>(null);

  // Meter readings
  readonly electricityOpening = signal(0);
  readonly electricityClosing = signal(0);
  readonly waterOpening = signal(0);
  readonly waterClosing = signal(0);
  readonly sanitationOpening = signal(0);
  readonly sanitationClosing = signal(0);

  // Tariff rates
  readonly electricityRate = signal(0);
  readonly waterRateTier1 = signal(0);
  readonly waterRateTier2 = signal(0);
  readonly waterRateTier3 = signal(0);
  readonly sanitationRateTier1 = signal(0);
  readonly sanitationRateTier2 = signal(0);
  readonly sanitationRateTier3 = signal(0);

  // Computed preview
  readonly preview = computed(() => this.billService.calculatePreview(
    this.electricityOpening(),
    this.electricityClosing(),
    this.waterOpening(),
    this.waterClosing(),
    this.sanitationOpening(),
    this.sanitationClosing(),
    this.electricityRate(),
    this.waterRateTier1(),
    this.waterRateTier2(),
    this.waterRateTier3(),
    this.sanitationRateTier1(),
    this.sanitationRateTier2(),
    this.sanitationRateTier3()
  ));

  // Property dropdown options
  readonly propertyOptions = computed(() =>
    this.propertyService.properties().map(p => ({
      label: `${p.name} - ${p.address}`,
      value: p.id
    }))
  );

  // Form validation
  readonly isFormValid = computed(() => !!(
    this.selectedProperty() &&
    this.periodStart() &&
    this.periodEnd() &&
    this.periodEnd()! >= this.periodStart()! &&
    this.electricityClosing() >= this.electricityOpening() &&
    this.waterClosing() >= this.waterOpening() &&
    this.sanitationClosing() >= this.sanitationOpening() &&
    this.electricityRate() >= 0 &&
    this.waterRateTier1() >= 0 &&
    this.waterRateTier2() >= 0 &&
    this.waterRateTier3() >= 0 &&
    this.sanitationRateTier1() >= 0 &&
    this.sanitationRateTier2() >= 0 &&
    this.sanitationRateTier3() >= 0
  ));

  ngOnInit(): void {
    this.propertyService.loadProperties();

    // Set default billing period (previous month)
    const today = new Date();
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 1);
    const lastMonthEnd = new Date(today.getFullYear(), today.getMonth(), 0);
    this.periodStart.set(lastMonth);
    this.periodEnd.set(lastMonthEnd);
  }

  onPropertyChange(propertyId: string | null): void {
    const property = propertyId
      ? this.propertyService.properties().find(p => p.id === propertyId) ?? null
      : null;

    this.selectedPropertyId.set(property ? propertyId : null);
    this.selectedProperty.set(property);

    // Pre-fill rates from property defaults (or reset to 0)
    this.electricityRate.set(property?.defaultElectricityRate ?? 0);
    this.waterRateTier1.set(property?.defaultWaterRateTier1 ?? 0);
    this.waterRateTier2.set(property?.defaultWaterRateTier2 ?? 0);
    this.waterRateTier3.set(property?.defaultWaterRateTier3 ?? 0);
    this.sanitationRateTier1.set(property?.defaultSanitationRateTier1 ?? 0);
    this.sanitationRateTier2.set(property?.defaultSanitationRateTier2 ?? 0);
    this.sanitationRateTier3.set(property?.defaultSanitationRateTier3 ?? 0);
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
      propertyId: this.selectedPropertyId()!,
      periodStart: this.formatDate(this.periodStart()!),
      periodEnd: this.formatDate(this.periodEnd()!),
      electricityReadingOpening: this.electricityOpening(),
      electricityReadingClosing: this.electricityClosing(),
      waterReadingOpening: this.waterOpening(),
      waterReadingClosing: this.waterClosing(),
      sanitationReadingOpening: this.sanitationOpening(),
      sanitationReadingClosing: this.sanitationClosing(),
      electricityRate: this.electricityRate(),
      waterRateTier1: this.waterRateTier1(),
      waterRateTier2: this.waterRateTier2(),
      waterRateTier3: this.waterRateTier3(),
      sanitationRateTier1: this.sanitationRateTier1(),
      sanitationRateTier2: this.sanitationRateTier2(),
      sanitationRateTier3: this.sanitationRateTier3()
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
    return formatDateToISO(date);
  }
}

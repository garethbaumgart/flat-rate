import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { Select } from 'primeng/select';
import { DatePicker } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { DividerModule } from 'primeng/divider';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PropertyService } from '../core/services/property.service';
import { BillService } from '../core/services/bill.service';
import { Bill } from '../core/models/bill.model';

@Component({
  selector: 'app-bills',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    ButtonModule,
    CardModule,
    TableModule,
    Select,
    DatePicker,
    ToastModule,
    ConfirmDialogModule,
    DialogModule,
    DividerModule
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="p-4 md:p-8">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-bold text-gray-900">Bills</h1>
          <p class="text-gray-600 mt-1">View and manage utility bills</p>
        </div>
        <p-button
          label="Create Bill"
          icon="pi pi-plus"
          routerLink="/bills/create"
        />
      </div>

      <!-- Filters -->
      <p-card styleClass="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="flex flex-col gap-2">
            <label for="filterProperty" class="font-medium text-sm">Property</label>
            <p-select
              id="filterProperty"
              [options]="propertyFilterOptions()"
              [ngModel]="filterPropertyId()"
              (ngModelChange)="filterPropertyId.set($event)"
              optionLabel="label"
              optionValue="value"
              placeholder="All properties"
              styleClass="w-full"
              [showClear]="true"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="filterStartDate" class="font-medium text-sm">From Date</label>
            <p-datepicker
              id="filterStartDate"
              [ngModel]="filterStartDate()"
              (ngModelChange)="filterStartDate.set($event)"
              dateFormat="yy-mm-dd"
              styleClass="w-full"
              [showIcon]="true"
              [showClear]="true"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="filterEndDate" class="font-medium text-sm">To Date</label>
            <p-datepicker
              id="filterEndDate"
              [ngModel]="filterEndDate()"
              (ngModelChange)="filterEndDate.set($event)"
              dateFormat="yy-mm-dd"
              styleClass="w-full"
              [showIcon]="true"
              [showClear]="true"
            />
          </div>
        </div>
      </p-card>

      @if (billService.loading()) {
        <div class="flex justify-center py-8">
          <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>
      } @else if (billService.error()) {
        <p-card>
          <div class="text-center py-8">
            <i class="pi pi-exclamation-triangle text-4xl text-red-500 mb-4"></i>
            <p class="text-gray-600">{{ billService.error() }}</p>
            <p-button
              label="Try Again"
              icon="pi pi-refresh"
              styleClass="mt-4"
              (onClick)="loadBills()"
            />
          </div>
        </p-card>
      } @else if (!billService.hasBills()) {
        <p-card>
          <div class="text-center py-8">
            <i class="pi pi-file text-4xl text-gray-400 mb-4"></i>
            <p class="text-gray-600 mb-4">No bills found. Create your first bill to get started.</p>
            <p-button
              label="Create Bill"
              icon="pi pi-plus"
              routerLink="/bills/create"
            />
          </div>
        </p-card>
      } @else {
        <p-card>
          <p-table
            [value]="filteredBills()"
            styleClass="p-datatable-sm"
            [paginator]="filteredBills().length > 10"
            [rows]="10"
            [rowsPerPageOptions]="[10, 25, 50]"
          >
            <ng-template pTemplate="header">
              <tr>
                <th>Invoice #</th>
                <th class="hidden md:table-cell">Property</th>
                <th>Period</th>
                <th class="text-right">Total</th>
                <th class="text-right">Actions</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-bill>
              <tr>
                <td>
                  <span class="font-medium">{{ bill.invoiceNumber }}</span>
                </td>
                <td class="hidden md:table-cell">{{ getPropertyName(bill.propertyId) }}</td>
                <td>
                  <span class="text-sm">
                    {{ formatDate(bill.periodStart) }} - {{ formatDate(bill.periodEnd) }}
                  </span>
                </td>
                <td class="text-right font-medium">
                  R {{ bill.total | number:'1.2-2' }}
                </td>
                <td class="text-right">
                  <div class="flex justify-end gap-2">
                    <p-button
                      icon="pi pi-eye"
                      [rounded]="true"
                      [text]="true"
                      severity="secondary"
                      pTooltip="View Details"
                      ariaLabel="View bill details"
                      (onClick)="viewBill(bill)"
                    />
                    <p-button
                      icon="pi pi-trash"
                      [rounded]="true"
                      [text]="true"
                      severity="danger"
                      pTooltip="Delete"
                      ariaLabel="Delete bill"
                      (onClick)="confirmDelete(bill)"
                    />
                  </div>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="5" class="text-center py-8 text-gray-500">
                  No bills match your filter criteria.
                </td>
              </tr>
            </ng-template>
          </p-table>
        </p-card>
      }
    </div>

    <!-- Bill Detail Dialog -->
    <p-dialog
      header="Bill Details"
      [(visible)]="showDetailDialog"
      [modal]="true"
      [style]="{ width: '90vw', maxWidth: '700px' }"
    >
      @if (selectedBill()) {
        <div class="flex flex-col gap-4">
          <!-- Header Info -->
          <div class="bg-gray-50 p-4 rounded">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <span class="text-sm text-gray-500">Invoice Number</span>
                <p class="font-bold text-lg">{{ selectedBill()?.invoiceNumber }}</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Property</span>
                <p class="font-medium">{{ getPropertyName(selectedBill()?.propertyId || '') }}</p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Billing Period</span>
                <p class="font-medium">
                  {{ formatDate(selectedBill()?.periodStart || '') }} - {{ formatDate(selectedBill()?.periodEnd || '') }}
                </p>
              </div>
              <div>
                <span class="text-sm text-gray-500">Created</span>
                <p class="font-medium">{{ formatDate(selectedBill()?.createdAt || '') }}</p>
              </div>
            </div>
          </div>

          <p-divider />

          <!-- Meter Readings -->
          <div>
            <h4 class="font-bold mb-3">Meter Readings</h4>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="border rounded p-3">
                <span class="text-sm text-gray-500">Electricity</span>
                <p class="font-medium">
                  {{ selectedBill()?.electricityReading?.opening }} → {{ selectedBill()?.electricityReading?.closing }}
                </p>
                <p class="text-primary font-bold">
                  {{ selectedBill()?.electricityReading?.unitsUsed | number:'1.2-2' }} kWh
                </p>
              </div>
              <div class="border rounded p-3">
                <span class="text-sm text-gray-500">Water</span>
                <p class="font-medium">
                  {{ selectedBill()?.waterReading?.opening }} → {{ selectedBill()?.waterReading?.closing }}
                </p>
                <p class="text-primary font-bold">
                  {{ selectedBill()?.waterReading?.unitsUsed | number:'1.2-2' }} kL
                </p>
              </div>
              <div class="border rounded p-3">
                <span class="text-sm text-gray-500">Sanitation</span>
                <p class="font-medium">
                  {{ selectedBill()?.sanitationReading?.opening }} → {{ selectedBill()?.sanitationReading?.closing }}
                </p>
                <p class="text-primary font-bold">
                  {{ selectedBill()?.sanitationReading?.unitsUsed | number:'1.2-2' }} kL
                </p>
              </div>
            </div>
          </div>

          <p-divider />

          <!-- Cost Breakdown -->
          <div>
            <h4 class="font-bold mb-3">Cost Breakdown</h4>
            <div class="flex flex-col gap-2">
              <div class="flex justify-between">
                <span class="text-gray-600">Electricity</span>
                <span class="font-medium">R {{ selectedBill()?.electricityCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">Water</span>
                <span class="font-medium">R {{ selectedBill()?.waterCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">Sanitation</span>
                <span class="font-medium">R {{ selectedBill()?.sanitationCost | number:'1.2-2' }}</span>
              </div>
              <p-divider />
              <div class="flex justify-between">
                <span class="text-gray-600">Subtotal</span>
                <span class="font-medium">R {{ selectedBill()?.subtotal | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">VAT (15%)</span>
                <span class="font-medium">R {{ selectedBill()?.vatAmount | number:'1.2-2' }}</span>
              </div>
              <p-divider />
              <div class="flex justify-between text-lg">
                <span class="font-bold">Total</span>
                <span class="font-bold text-primary">R {{ selectedBill()?.total | number:'1.2-2' }}</span>
              </div>
            </div>
          </div>
        </div>
      }
      <ng-template pTemplate="footer">
        <p-button
          label="Close"
          icon="pi pi-times"
          [text]="true"
          (onClick)="closeDetailDialog()"
        />
      </ng-template>
    </p-dialog>

    <p-toast />
    <p-confirmDialog />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BillsPage implements OnInit {
  readonly propertyService = inject(PropertyService);
  readonly billService = inject(BillService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  // Filters (as signals for reactive computed)
  filterPropertyId = signal<string | null>(null);
  filterStartDate = signal<Date | null>(null);
  filterEndDate = signal<Date | null>(null);

  // Detail dialog
  showDetailDialog = false;
  selectedBill = signal<Bill | null>(null);

  // Property map for quick lookups
  private propertyMap = signal<Map<string, string>>(new Map());

  // Property filter options
  propertyFilterOptions = computed(() => [
    { label: 'All Properties', value: null },
    ...this.propertyService.properties().map(p => ({
      label: p.name,
      value: p.id
    }))
  ]);

  // Filtered bills
  filteredBills = computed(() => {
    let bills = this.billService.bills();
    const propertyId = this.filterPropertyId();
    const startDate = this.filterStartDate();
    const endDate = this.filterEndDate();

    if (propertyId) {
      bills = bills.filter(b => b.propertyId === propertyId);
    }

    if (startDate) {
      const startDateStr = startDate.toISOString().split('T')[0];
      bills = bills.filter(b => b.periodStart >= startDateStr);
    }

    if (endDate) {
      const endDateStr = endDate.toISOString().split('T')[0];
      bills = bills.filter(b => b.periodEnd <= endDateStr);
    }

    // Sort by period end date descending (most recent first)
    return [...bills].sort((a, b) =>
      new Date(b.periodEnd).getTime() - new Date(a.periodEnd).getTime()
    );
  });

  async ngOnInit(): Promise<void> {
    await Promise.all([
      this.propertyService.loadProperties(),
      this.billService.loadBills()
    ]);

    // Build property map for quick lookups
    const map = new Map<string, string>();
    this.propertyService.properties().forEach(p => map.set(p.id, p.name));
    this.propertyMap.set(map);
  }

  async loadBills(): Promise<void> {
    await this.billService.loadBills();
  }

  getPropertyName(propertyId: string): string {
    return this.propertyMap().get(propertyId) || 'Unknown Property';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-ZA', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  viewBill(bill: Bill): void {
    this.selectedBill.set(bill);
    this.showDetailDialog = true;
  }

  closeDetailDialog(): void {
    this.showDetailDialog = false;
    this.selectedBill.set(null);
  }

  confirmDelete(bill: Bill): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete invoice "${bill.invoiceNumber}"? This action cannot be undone.`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: async () => {
        const success = await this.billService.deleteBill(bill.id);

        if (success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Bill deleted successfully.'
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.billService.error() || 'Failed to delete bill.'
          });
        }
      }
    });
  }
}

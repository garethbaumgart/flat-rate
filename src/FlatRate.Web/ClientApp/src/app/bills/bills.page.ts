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
import { formatDateToISO } from '../core/utils/date-utils';

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
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
        <div>
          <h1 class="text-2xl md:text-3xl font-bold" style="color: var(--color-text-primary);">Bills</h1>
          <p style="color: var(--color-text-secondary);" class="mt-1">View and manage utility bills</p>
        </div>
        <p-button
          label="Create Bill"
          icon="pi pi-plus"
          routerLink="/bills/create"
        />
      </div>

      <!-- Filters -->
      <div class="rounded-xl border p-4 mb-6" style="background: var(--color-bg-card); border-color: var(--color-border);">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="flex flex-col gap-2">
            <label for="filterProperty" class="font-medium text-sm" style="color: var(--color-text-primary);">Property</label>
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
            <label for="filterStartDate" class="font-medium text-sm" style="color: var(--color-text-primary);">From Date</label>
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
            <label for="filterEndDate" class="font-medium text-sm" style="color: var(--color-text-primary);">To Date</label>
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
      </div>

      @if (billService.loading()) {
        <div class="flex justify-center py-16">
          <div class="flex flex-col items-center gap-4">
            <i class="pi pi-spin pi-spinner text-4xl" style="color: var(--color-accent);"></i>
            <p style="color: var(--color-text-secondary);">Loading bills...</p>
          </div>
        </div>
      } @else if (billService.error()) {
        <div class="rounded-xl border p-8 text-center" style="background: var(--color-bg-card); border-color: var(--color-border);">
          <i class="pi pi-exclamation-triangle text-4xl mb-4" style="color: var(--color-error);"></i>
          <p style="color: var(--color-text-secondary);" class="mb-4">{{ billService.error() }}</p>
          <p-button
            label="Try Again"
            icon="pi pi-refresh"
            severity="secondary"
            (onClick)="loadBills()"
          />
        </div>
      } @else if (!billService.hasBills()) {
        <div class="rounded-xl border p-12 text-center" style="background: var(--color-bg-card); border-color: var(--color-border);">
          <div class="w-16 h-16 rounded-xl flex items-center justify-center mx-auto mb-6" style="background: var(--color-accent-bg);">
            <i class="pi pi-file text-3xl" style="color: var(--color-accent);"></i>
          </div>
          <h3 class="text-lg font-semibold mb-2" style="color: var(--color-text-primary);">No bills yet</h3>
          <p style="color: var(--color-text-secondary);" class="mb-6 max-w-md mx-auto">Create your first bill to start tracking utility usage.</p>
          <p-button
            label="Create Bill"
            icon="pi pi-plus"
            routerLink="/bills/create"
          />
        </div>
      } @else {
        <div class="rounded-xl border overflow-hidden" style="background: var(--color-bg-card); border-color: var(--color-border);">
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
                <td colspan="5" class="text-center py-8" style="color: var(--color-text-muted);">
                  No bills match your filter criteria.
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      }
    </div>

    <!-- Bill Detail Dialog -->
    <p-dialog
      header="Bill Details"
      [(visible)]="showDetailDialog"
      [modal]="true"
      [maximizable]="true"
      [style]="{ width: '90vw', maxWidth: '700px' }"
      [contentStyle]="{ 'max-height': '70vh', 'overflow-y': 'auto' }"
    >
      @if (selectedBill()) {
        <div class="flex flex-col gap-4">
          <!-- Header Info -->
          <div class="p-4 rounded-lg" style="background: var(--color-bg-tertiary);">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <span class="text-sm" style="color: var(--color-text-muted);">Invoice Number</span>
                <p class="font-bold text-lg" style="color: var(--color-text-primary);">{{ selectedBill()?.invoiceNumber }}</p>
              </div>
              <div>
                <span class="text-sm" style="color: var(--color-text-muted);">Property</span>
                <p class="font-medium" style="color: var(--color-text-primary);">{{ getPropertyName(selectedBill()?.propertyId || '') }}</p>
              </div>
              <div>
                <span class="text-sm" style="color: var(--color-text-muted);">Billing Period</span>
                <p class="font-medium" style="color: var(--color-text-primary);">
                  {{ formatDate(selectedBill()?.periodStart || '') }} - {{ formatDate(selectedBill()?.periodEnd || '') }}
                </p>
              </div>
              <div>
                <span class="text-sm" style="color: var(--color-text-muted);">Created</span>
                <p class="font-medium" style="color: var(--color-text-primary);">{{ formatDate(selectedBill()?.createdAt || '') }}</p>
              </div>
            </div>
          </div>

          <p-divider />

          <!-- Meter Readings -->
          <div>
            <h4 class="font-bold mb-3" style="color: var(--color-text-primary);">Meter Readings</h4>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="rounded-lg p-3" style="border: 1px solid var(--color-border);">
                <span class="text-sm" style="color: var(--color-text-muted);">Electricity</span>
                <p class="font-medium" style="color: var(--color-text-secondary);">
                  {{ selectedBill()?.electricityReading?.opening }} → {{ selectedBill()?.electricityReading?.closing }}
                </p>
                <p class="font-bold" style="color: var(--color-accent);">
                  {{ selectedBill()?.electricityReading?.unitsUsed | number:'1.2-2' }} kWh
                </p>
              </div>
              <div class="rounded-lg p-3" style="border: 1px solid var(--color-border);">
                <span class="text-sm" style="color: var(--color-text-muted);">Water</span>
                <p class="font-medium" style="color: var(--color-text-secondary);">
                  {{ selectedBill()?.waterReading?.opening }} → {{ selectedBill()?.waterReading?.closing }}
                </p>
                <p class="font-bold" style="color: var(--color-accent);">
                  {{ selectedBill()?.waterReading?.unitsUsed | number:'1.2-2' }} kL
                </p>
              </div>
              <div class="rounded-lg p-3" style="border: 1px solid var(--color-border);">
                <span class="text-sm" style="color: var(--color-text-muted);">Sanitation</span>
                <p class="font-medium" style="color: var(--color-text-secondary);">
                  {{ selectedBill()?.sanitationReading?.opening }} → {{ selectedBill()?.sanitationReading?.closing }}
                </p>
                <p class="font-bold" style="color: var(--color-accent);">
                  {{ selectedBill()?.sanitationReading?.unitsUsed | number:'1.2-2' }} kL
                </p>
              </div>
            </div>
          </div>

          <p-divider />

          <!-- Cost Breakdown -->
          <div>
            <h4 class="font-bold mb-3" style="color: var(--color-text-primary);">Cost Breakdown</h4>
            <div class="flex flex-col gap-2">
              <div class="flex justify-between">
                <span style="color: var(--color-text-secondary);">Electricity</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ selectedBill()?.electricityCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between">
                <span style="color: var(--color-text-secondary);">Water</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ selectedBill()?.waterCost | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between">
                <span style="color: var(--color-text-secondary);">Sanitation</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ selectedBill()?.sanitationCost | number:'1.2-2' }}</span>
              </div>
              <p-divider />
              <div class="flex justify-between">
                <span style="color: var(--color-text-secondary);">Subtotal</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ selectedBill()?.subtotal | number:'1.2-2' }}</span>
              </div>
              <div class="flex justify-between">
                <span style="color: var(--color-text-secondary);">VAT (15%)</span>
                <span class="font-medium" style="color: var(--color-text-primary);">R {{ selectedBill()?.vatAmount | number:'1.2-2' }}</span>
              </div>
              <p-divider />
              <div class="flex justify-between text-lg">
                <span class="font-bold" style="color: var(--color-text-primary);">Total</span>
                <span class="font-bold" style="color: var(--color-accent);">R {{ selectedBill()?.total | number:'1.2-2' }}</span>
              </div>
            </div>
          </div>
        </div>
      }
      <ng-template pTemplate="footer">
        <div class="flex justify-between w-full">
          <p-button
            label="Download PDF"
            icon="pi pi-file-pdf"
            severity="secondary"
            [loading]="downloadingPdf()"
            (onClick)="downloadPdf()"
          />
          <p-button
            label="Close"
            icon="pi pi-times"
            [text]="true"
            (onClick)="closeDetailDialog()"
          />
        </div>
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
  downloadingPdf = signal(false);

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
      const startDateStr = formatDateToISO(startDate);
      bills = bills.filter(b => b.periodStart >= startDateStr);
    }

    if (endDate) {
      const endDateStr = formatDateToISO(endDate);
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

  async downloadPdf(): Promise<void> {
    const bill = this.selectedBill();
    if (!bill) return;

    this.downloadingPdf.set(true);

    try {
      const blob = await this.billService.getInvoicePdfBlob(bill.id);

      // Create download link and append to DOM for browser compatibility
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Invoice-${bill.invoiceNumber}.pdf`;
      document.body.appendChild(link);
      link.click();

      // Clean up the link element and object URL asynchronously to avoid
      // cancelling the download in some browsers.
      setTimeout(() => {
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      }, 0);

      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'PDF downloaded successfully.'
      });
    } catch {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to download PDF.'
      });
    } finally {
      this.downloadingPdf.set(false);
    }
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

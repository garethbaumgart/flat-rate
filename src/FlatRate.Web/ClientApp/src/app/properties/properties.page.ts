import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PropertyService } from '../core/services/property.service';
import { Property, CreatePropertyRequest, SetPropertyRatesRequest } from '../core/models/property.model';

@Component({
  selector: 'app-properties',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    TableModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    ToastModule,
    ConfirmDialogModule
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="p-4 md:p-8">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-bold text-gray-900">Properties</h1>
          <p class="text-gray-600 mt-1">Manage your rental properties</p>
        </div>
        <p-button
          label="Add Property"
          icon="pi pi-plus"
          (onClick)="openCreateDialog()"
        />
      </div>

      @if (propertyService.loading()) {
        <div class="flex justify-center py-8">
          <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>
      } @else if (propertyService.error()) {
        <p-card>
          <div class="text-center py-8">
            <i class="pi pi-exclamation-triangle text-4xl text-red-500 mb-4"></i>
            <p class="text-gray-600">{{ propertyService.error() }}</p>
            <p-button
              label="Try Again"
              icon="pi pi-refresh"
              styleClass="mt-4"
              (onClick)="loadProperties()"
            />
          </div>
        </p-card>
      } @else if (!propertyService.hasProperties()) {
        <p-card>
          <div class="text-center py-8">
            <i class="pi pi-building text-4xl text-gray-400 mb-4"></i>
            <p class="text-gray-600 mb-4">No properties yet. Add your first property to get started.</p>
            <p-button
              label="Add Property"
              icon="pi pi-plus"
              (onClick)="openCreateDialog()"
            />
          </div>
        </p-card>
      } @else {
        <p-card>
          <p-table
            [value]="propertyService.properties()"
            styleClass="p-datatable-sm"
          >
            <ng-template pTemplate="header">
              <tr>
                <th>Name</th>
                <th class="hidden md:table-cell">Address</th>
                <th class="hidden lg:table-cell">Default Rates</th>
                <th class="text-right">Actions</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-property>
              <tr>
                <td>
                  <div class="font-medium">{{ property.name }}</div>
                  <div class="text-sm text-gray-500 md:hidden">{{ property.address }}</div>
                </td>
                <td class="hidden md:table-cell">{{ property.address }}</td>
                <td class="hidden lg:table-cell">
                  @if (hasDefaultRates(property)) {
                    <span class="text-green-600">
                      <i class="pi pi-check-circle mr-1"></i>Configured
                    </span>
                  } @else {
                    <span class="text-gray-400">
                      <i class="pi pi-minus-circle mr-1"></i>Not set
                    </span>
                  }
                </td>
                <td class="text-right">
                  <div class="flex justify-end gap-2">
                    <p-button
                      icon="pi pi-pencil"
                      [rounded]="true"
                      [text]="true"
                      severity="secondary"
                      pTooltip="Edit"
                      ariaLabel="Edit property"
                      (onClick)="openEditDialog(property)"
                    />
                    <p-button
                      icon="pi pi-cog"
                      [rounded]="true"
                      [text]="true"
                      severity="secondary"
                      pTooltip="Set Rates"
                      ariaLabel="Set rates for property"
                      (onClick)="openRatesDialog(property)"
                    />
                    <p-button
                      icon="pi pi-trash"
                      [rounded]="true"
                      [text]="true"
                      severity="danger"
                      pTooltip="Delete"
                      ariaLabel="Delete property"
                      (onClick)="confirmDelete(property)"
                    />
                  </div>
                </td>
              </tr>
            </ng-template>
          </p-table>
        </p-card>
      }
    </div>

    <!-- Create/Edit Property Dialog -->
    <p-dialog
      [header]="isEditMode() ? 'Edit Property' : 'Add Property'"
      [(visible)]="showPropertyDialog"
      [modal]="true"
      [style]="{ width: '90vw', maxWidth: '500px' }"
    >
      <div class="flex flex-col gap-4">
        <div class="flex flex-col gap-2">
          <label for="name" class="font-medium">Name *</label>
          <input
            pInputText
            id="name"
            [(ngModel)]="propertyForm.name"
            placeholder="Property name"
            class="w-full"
          />
        </div>
        <div class="flex flex-col gap-2">
          <label for="address" class="font-medium">Address *</label>
          <input
            pInputText
            id="address"
            [(ngModel)]="propertyForm.address"
            placeholder="Property address"
            class="w-full"
          />
        </div>
      </div>
      <ng-template pTemplate="footer">
        <p-button
          label="Cancel"
          icon="pi pi-times"
          [text]="true"
          (onClick)="closePropertyDialog()"
        />
        <p-button
          [label]="isEditMode() ? 'Update' : 'Create'"
          icon="pi pi-check"
          [loading]="propertyService.loading()"
          (onClick)="saveProperty()"
        />
      </ng-template>
    </p-dialog>

    <!-- Set Rates Dialog -->
    <p-dialog
      header="Set Default Rates"
      [(visible)]="showRatesDialog"
      [modal]="true"
      [style]="{ width: '90vw', maxWidth: '600px' }"
    >
      @if (selectedProperty()) {
        <div class="mb-4">
          <p class="text-gray-600">
            Setting default rates for <strong>{{ selectedProperty()?.name }}</strong>
          </p>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="electricityRate" class="font-medium">Electricity Rate (per kWh)</label>
            <p-inputNumber
              id="electricityRate"
              [(ngModel)]="ratesForm.electricityRate"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>
          <div></div>

          <div class="flex flex-col gap-2">
            <label for="waterTier1" class="font-medium">Water Tier 1 (0-6 kL)</label>
            <p-inputNumber
              id="waterTier1"
              [(ngModel)]="ratesForm.waterRateTier1"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="sanitationTier1" class="font-medium">Sanitation Tier 1 (0-6 kL)</label>
            <p-inputNumber
              id="sanitationTier1"
              [(ngModel)]="ratesForm.sanitationRateTier1"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>

          <div class="flex flex-col gap-2">
            <label for="waterTier2" class="font-medium">Water Tier 2 (7-15 kL)</label>
            <p-inputNumber
              id="waterTier2"
              [(ngModel)]="ratesForm.waterRateTier2"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="sanitationTier2" class="font-medium">Sanitation Tier 2 (7-15 kL)</label>
            <p-inputNumber
              id="sanitationTier2"
              [(ngModel)]="ratesForm.sanitationRateTier2"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>

          <div class="flex flex-col gap-2">
            <label for="waterTier3" class="font-medium">Water Tier 3 (16+ kL)</label>
            <p-inputNumber
              id="waterTier3"
              [(ngModel)]="ratesForm.waterRateTier3"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="sanitationTier3" class="font-medium">Sanitation Tier 3 (16+ kL)</label>
            <p-inputNumber
              id="sanitationTier3"
              [(ngModel)]="ratesForm.sanitationRateTier3"
              [minFractionDigits]="2"
              [maxFractionDigits]="4"
              prefix="R "
              class="w-full"
            />
          </div>
        </div>
      }
      <ng-template pTemplate="footer">
        <p-button
          label="Cancel"
          icon="pi pi-times"
          [text]="true"
          (onClick)="closeRatesDialog()"
        />
        <p-button
          label="Save Rates"
          icon="pi pi-check"
          [loading]="propertyService.loading()"
          (onClick)="saveRates()"
        />
      </ng-template>
    </p-dialog>

    <p-toast />
    <p-confirmDialog />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PropertiesPage implements OnInit {
  readonly propertyService = inject(PropertyService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  showPropertyDialog = false;
  showRatesDialog = false;
  selectedProperty = signal<Property | null>(null);
  isEditMode = signal(false);

  propertyForm = {
    name: '',
    address: ''
  };

  ratesForm: SetPropertyRatesRequest = {
    electricityRate: null,
    waterRateTier1: null,
    waterRateTier2: null,
    waterRateTier3: null,
    sanitationRateTier1: null,
    sanitationRateTier2: null,
    sanitationRateTier3: null
  };

  ngOnInit(): void {
    this.loadProperties();
  }

  async loadProperties(): Promise<void> {
    await this.propertyService.loadProperties();
  }

  hasDefaultRates(property: Property): boolean {
    return property.defaultElectricityRate !== null ||
           property.defaultWaterRateTier1 !== null ||
           property.defaultWaterRateTier2 !== null ||
           property.defaultWaterRateTier3 !== null ||
           property.defaultSanitationRateTier1 !== null ||
           property.defaultSanitationRateTier2 !== null ||
           property.defaultSanitationRateTier3 !== null;
  }

  openCreateDialog(): void {
    this.isEditMode.set(false);
    this.selectedProperty.set(null);
    this.propertyForm = { name: '', address: '' };
    this.showPropertyDialog = true;
  }

  openEditDialog(property: Property): void {
    this.isEditMode.set(true);
    this.selectedProperty.set(property);
    this.propertyForm = {
      name: property.name,
      address: property.address
    };
    this.showPropertyDialog = true;
  }

  closePropertyDialog(): void {
    this.showPropertyDialog = false;
    this.selectedProperty.set(null);
  }

  async saveProperty(): Promise<void> {
    if (!this.propertyForm.name.trim() || !this.propertyForm.address.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Name and address are required.'
      });
      return;
    }

    if (this.isEditMode() && this.selectedProperty()) {
      const success = await this.propertyService.updateProperty(
        this.selectedProperty()!.id,
        { name: this.propertyForm.name.trim(), address: this.propertyForm.address.trim() }
      );

      if (success) {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Property updated successfully.'
        });
        this.closePropertyDialog();
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: this.propertyService.error() || 'Failed to update property.'
        });
      }
    } else {
      const id = await this.propertyService.createProperty({
        name: this.propertyForm.name.trim(),
        address: this.propertyForm.address.trim()
      });

      if (id) {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Property created successfully.'
        });
        this.closePropertyDialog();
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: this.propertyService.error() || 'Failed to create property.'
        });
      }
    }
  }

  openRatesDialog(property: Property): void {
    this.selectedProperty.set(property);
    this.ratesForm = {
      electricityRate: property.defaultElectricityRate,
      waterRateTier1: property.defaultWaterRateTier1,
      waterRateTier2: property.defaultWaterRateTier2,
      waterRateTier3: property.defaultWaterRateTier3,
      sanitationRateTier1: property.defaultSanitationRateTier1,
      sanitationRateTier2: property.defaultSanitationRateTier2,
      sanitationRateTier3: property.defaultSanitationRateTier3
    };
    this.showRatesDialog = true;
  }

  closeRatesDialog(): void {
    this.showRatesDialog = false;
    this.selectedProperty.set(null);
  }

  async saveRates(): Promise<void> {
    if (!this.selectedProperty()) return;

    const success = await this.propertyService.setPropertyRates(
      this.selectedProperty()!.id,
      this.ratesForm
    );

    if (success) {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Rates updated successfully.'
      });
      this.closeRatesDialog();
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: this.propertyService.error() || 'Failed to update rates.'
      });
    }
  }

  confirmDelete(property: Property): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${property.name}"? This action cannot be undone.`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: async () => {
        const success = await this.propertyService.deleteProperty(property.id);

        if (success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Property deleted successfully.'
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.propertyService.error() || 'Failed to delete property.'
          });
        }
      }
    });
  }
}

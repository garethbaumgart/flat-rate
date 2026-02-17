import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PropertyService } from '../core/services/property.service';
import { PropertySharingService } from '../core/services/property-sharing.service';
import { Property, SetPropertyRatesRequest } from '../core/models/property.model';

@Component({
  selector: 'app-properties',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    Tooltip
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Page Header -->
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
        <div>
          <h1 class="text-2xl md:text-3xl font-bold" style="color: var(--color-text-primary);">
            Properties
          </h1>
          <p style="color: var(--color-text-secondary);" class="mt-1">
            Manage your rental properties and default rates
          </p>
        </div>
        <p-button
          label="Add Property"
          icon="pi pi-plus"
          (onClick)="openCreateDialog()"
        />
      </div>

      @if (propertyService.loading()) {
        <div class="flex justify-center py-16">
          <div class="flex flex-col items-center gap-4">
            <i class="pi pi-spin pi-spinner text-4xl" style="color: var(--color-accent);"></i>
            <p style="color: var(--color-text-secondary);">Loading properties...</p>
          </div>
        </div>
      } @else if (propertyService.error()) {
        <div
          class="rounded-xl border p-8 text-center"
          style="background: var(--color-bg-card); border-color: var(--color-border);"
        >
          <i class="pi pi-exclamation-triangle text-4xl mb-4" style="color: var(--color-error);"></i>
          <p style="color: var(--color-text-secondary);" class="mb-4">{{ propertyService.error() }}</p>
          <p-button
            label="Try Again"
            icon="pi pi-refresh"
            severity="secondary"
            (onClick)="loadProperties()"
          />
        </div>
      } @else if (!propertyService.hasProperties()) {
        <div
          class="rounded-xl border p-12 text-center"
          style="background: var(--color-bg-card); border-color: var(--color-border);"
        >
          <div
            class="w-16 h-16 rounded-xl flex items-center justify-center mx-auto mb-6"
            style="background: var(--color-accent-bg);"
          >
            <i class="pi pi-building text-3xl" style="color: var(--color-accent);"></i>
          </div>
          <h3 class="text-lg font-semibold mb-2" style="color: var(--color-text-primary);">
            No properties yet
          </h3>
          <p style="color: var(--color-text-secondary);" class="mb-6 max-w-md mx-auto">
            Add your first property to start managing utility rates and generating bills.
          </p>
          <p-button
            label="Add Property"
            icon="pi pi-plus"
            (onClick)="openCreateDialog()"
          />
        </div>
      } @else {
        <div
          class="rounded-xl border overflow-hidden"
          style="background: var(--color-bg-card); border-color: var(--color-border);"
        >
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
                  <div class="flex items-center gap-2">
                    <span class="font-medium" style="color: var(--color-text-primary);">{{ property.name }}</span>
                    @if (property.currentUserRole === 'Editor') {
                      <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                        style="background: var(--color-bg-tertiary); color: var(--color-text-muted);">
                        Shared
                      </span>
                    }
                  </div>
                  <div class="text-sm md:hidden" style="color: var(--color-text-secondary);">{{ property.address }}</div>
                </td>
                <td class="hidden md:table-cell" style="color: var(--color-text-secondary);">{{ property.address }}</td>
                <td class="hidden lg:table-cell">
                  @if (hasDefaultRates(property)) {
                    <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium"
                      style="background: var(--color-success-bg); color: var(--color-success);">
                      <i class="pi pi-check-circle"></i>Configured
                    </span>
                  } @else {
                    <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium"
                      style="background: var(--color-bg-tertiary); color: var(--color-text-muted);">
                      <i class="pi pi-minus-circle"></i>Not set
                    </span>
                  }
                </td>
                <td class="text-right">
                  <div class="flex justify-end gap-1">
                    @if (property.currentUserRole === 'Owner') {
                      <p-button
                        icon="pi pi-users"
                        [rounded]="true"
                        [text]="true"
                        severity="secondary"
                        pTooltip="Share"
                        ariaLabel="Share property"
                        (onClick)="openCollaboratorsDialog(property)"
                      />
                    }
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
                    @if (property.currentUserRole === 'Owner') {
                      <p-button
                        icon="pi pi-trash"
                        [rounded]="true"
                        [text]="true"
                        severity="danger"
                        pTooltip="Delete"
                        ariaLabel="Delete property"
                        (onClick)="confirmDelete(property)"
                      />
                    }
                  </div>
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
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
          <label for="name" class="font-medium" style="color: var(--color-text-primary);">Name *</label>
          <input
            pInputText
            id="name"
            [(ngModel)]="propertyForm.name"
            placeholder="Property name"
            class="w-full"
          />
        </div>
        <div class="flex flex-col gap-2">
          <label for="address" class="font-medium" style="color: var(--color-text-primary);">Address *</label>
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
          severity="secondary"
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
      [style]="{ width: '90vw', maxWidth: '700px' }"
    >
      @if (selectedProperty()) {
        <div class="mb-6 p-4 rounded-lg" style="background: var(--color-bg-tertiary);">
          <p style="color: var(--color-text-secondary);">
            Setting default rates for <strong style="color: var(--color-text-primary);">{{ selectedProperty()?.name }}</strong>
            <i class="pi pi-info-circle text-sm ml-2" style="color: var(--color-text-muted);"
              pTooltip="Default rates are pre-filled when creating bills for this property. You can override them per bill."
              tooltipPosition="top"></i>
          </p>
        </div>
        <div class="flex flex-col gap-4">
          <!-- Electricity -->
          <div class="relative rounded-[10px] border-[1.5px] p-4 pt-5" style="border-color: var(--color-border);"
               role="group" aria-labelledby="rates-electricity-legend">
            <div id="rates-electricity-legend" class="absolute -top-2.5 left-4 px-2 text-xs font-semibold uppercase tracking-wide"
                 style="background: var(--color-bg-card); color: var(--color-warning);">
              <i class="pi pi-bolt"></i> Electricity
            </div>
            <div class="grid grid-cols-1 gap-4">
              <div class="flex flex-col gap-2">
                <label for="electricityRate" class="font-medium" style="color: var(--color-text-primary);">Rate (per kWh)</label>
                <p-inputNumber
                  id="electricityRate"
                  [(ngModel)]="ratesForm.electricityRate"
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
               role="group" aria-labelledby="rates-water-legend">
            <div id="rates-water-legend" class="absolute -top-2.5 left-4 px-2 text-xs font-semibold uppercase tracking-wide"
                 style="background: var(--color-bg-card); color: var(--color-info);">
              <i class="pi pi-wave-pulse"></i> Water
            </div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="flex flex-col gap-2">
                <label for="waterTier1" class="font-medium" style="color: var(--color-text-primary);">Tier 1 (0–6 kL)</label>
                <p-inputNumber
                  id="waterTier1"
                  [(ngModel)]="ratesForm.waterRateTier1"
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
                <label for="waterTier2" class="font-medium" style="color: var(--color-text-primary);">Tier 2 (7–15 kL)</label>
                <p-inputNumber
                  id="waterTier2"
                  [(ngModel)]="ratesForm.waterRateTier2"
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
                <label for="waterTier3" class="font-medium" style="color: var(--color-text-primary);">Tier 3 (16+ kL)</label>
                <p-inputNumber
                  id="waterTier3"
                  [(ngModel)]="ratesForm.waterRateTier3"
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
               role="group" aria-labelledby="rates-sanitation-legend">
            <div id="rates-sanitation-legend" class="absolute -top-2.5 left-4 px-2 text-xs font-semibold uppercase tracking-wide"
                 style="background: var(--color-bg-card); color: var(--color-success);">
              <i class="pi pi-sync"></i> Sanitation
            </div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="flex flex-col gap-2">
                <label for="sanitationTier1" class="font-medium" style="color: var(--color-text-primary);">Tier 1 (0–6 kL)</label>
                <p-inputNumber
                  id="sanitationTier1"
                  [(ngModel)]="ratesForm.sanitationRateTier1"
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
                <label for="sanitationTier2" class="font-medium" style="color: var(--color-text-primary);">Tier 2 (7–15 kL)</label>
                <p-inputNumber
                  id="sanitationTier2"
                  [(ngModel)]="ratesForm.sanitationRateTier2"
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
                <label for="sanitationTier3" class="font-medium" style="color: var(--color-text-primary);">Tier 3 (16+ kL)</label>
                <p-inputNumber
                  id="sanitationTier3"
                  [(ngModel)]="ratesForm.sanitationRateTier3"
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
      }
      <ng-template pTemplate="footer">
        <p-button
          label="Cancel"
          icon="pi pi-times"
          [text]="true"
          severity="secondary"
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

    <!-- Collaborators Dialog -->
    <p-dialog
      header="Share Property"
      [(visible)]="showCollaboratorsDialog"
      [modal]="true"
      [style]="{ width: '90vw', maxWidth: '600px' }"
    >
      @if (selectedProperty()) {
        <div class="mb-6 p-4 rounded-lg" style="background: var(--color-bg-tertiary);">
          <p style="color: var(--color-text-secondary);">
            Manage access to <strong style="color: var(--color-text-primary);">{{ selectedProperty()?.name }}</strong>
          </p>
        </div>

        <!-- Invite Form -->
        <div class="mb-6 flex gap-2">
          <input
            pInputText
            [(ngModel)]="inviteEmail"
            placeholder="Enter email address"
            class="flex-1"
            type="email"
          />
          <p-button
            label="Invite"
            icon="pi pi-user-plus"
            [loading]="sharingService.loading()"
            (onClick)="inviteCollaborator()"
          />
        </div>

        <!-- Collaborators List -->
        @if (sharingService.loading() && sharingService.collaborators().length === 0) {
          <div class="flex justify-center py-8">
            <i class="pi pi-spin pi-spinner text-2xl" style="color: var(--color-accent);"></i>
          </div>
        } @else if (sharingService.collaborators().length === 0) {
          <div class="text-center py-8" style="color: var(--color-text-secondary);">
            <i class="pi pi-users text-4xl mb-4" style="opacity: 0.5;"></i>
            <p>No collaborators yet. Invite someone to share this property.</p>
          </div>
        } @else {
          <div class="divide-y" style="border-color: var(--color-border);">
            @for (collaborator of sharingService.collaborators(); track collaborator.userId || collaborator.email) {
              <div class="flex items-center justify-between py-3">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-full flex items-center justify-center"
                    style="background: var(--color-accent-bg);">
                    <i class="pi pi-user" style="color: var(--color-accent);"></i>
                  </div>
                  <div>
                    <div class="font-medium" style="color: var(--color-text-primary);">
                      {{ collaborator.name || collaborator.email }}
                    </div>
                    @if (collaborator.name && collaborator.email) {
                      <div class="text-sm" style="color: var(--color-text-secondary);">
                        {{ collaborator.email }}
                      </div>
                    }
                    @if (collaborator.isPending) {
                      <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium mt-1"
                        style="background: var(--color-warning-bg); color: var(--color-warning);">
                        Pending
                      </span>
                    }
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <span class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium"
                    [style.background]="collaborator.role === 'Owner' ? 'var(--color-accent-bg)' : 'var(--color-bg-tertiary)'"
                    [style.color]="collaborator.role === 'Owner' ? 'var(--color-accent)' : 'var(--color-text-muted)'">
                    {{ collaborator.role }}
                    <i class="pi pi-info-circle"
                      [pTooltip]="collaborator.role === 'Owner'
                        ? 'Full access \u2014 can edit, set rates, create bills, manage sharing, and delete the property.'
                        : 'Can view, edit, set rates, and create bills. Cannot delete the property or manage sharing.'"
                      tooltipPosition="top"></i>
                  </span>
                  @if (collaborator.role !== 'Owner' && collaborator.userId) {
                    <p-button
                      icon="pi pi-times"
                      [rounded]="true"
                      [text]="true"
                      severity="danger"
                      pTooltip="Remove access"
                      ariaLabel="Remove access"
                      (onClick)="confirmRevokeAccess(collaborator)"
                    />
                  }
                </div>
              </div>
            }
          </div>
        }

        @if (sharingService.error()) {
          <div class="mt-4 p-3 rounded-lg" style="background: var(--color-error-bg);">
            <p class="text-sm" style="color: var(--color-error);">{{ sharingService.error() }}</p>
          </div>
        }
      }
      <ng-template pTemplate="footer">
        <p-button
          label="Close"
          [text]="true"
          severity="secondary"
          (onClick)="closeCollaboratorsDialog()"
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
  readonly sharingService = inject(PropertySharingService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  showPropertyDialog = false;
  showRatesDialog = false;
  showCollaboratorsDialog = false;
  selectedProperty = signal<Property | null>(null);
  isEditMode = signal(false);
  inviteEmail = '';

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

  async openCollaboratorsDialog(property: Property): Promise<void> {
    this.selectedProperty.set(property);
    this.inviteEmail = '';
    this.sharingService.clearCollaborators();
    this.showCollaboratorsDialog = true;
    await this.sharingService.loadCollaborators(property.id);
  }

  closeCollaboratorsDialog(): void {
    this.showCollaboratorsDialog = false;
    this.selectedProperty.set(null);
    this.inviteEmail = '';
    this.sharingService.clearCollaborators();
  }

  async inviteCollaborator(): Promise<void> {
    if (!this.selectedProperty() || !this.inviteEmail.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please enter an email address.'
      });
      return;
    }

    const success = await this.sharingService.inviteCollaborator(
      this.selectedProperty()!.id,
      { email: this.inviteEmail.trim() }
    );

    if (success) {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Invitation sent successfully.'
      });
      this.inviteEmail = '';
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: this.sharingService.error() || 'Failed to send invitation.'
      });
    }
  }

  confirmRevokeAccess(collaborator: { userId: string | null; name: string | null; email: string | null }): void {
    if (!collaborator.userId) return;

    const displayName = collaborator.name || collaborator.email || 'this user';

    this.confirmationService.confirm({
      message: `Are you sure you want to remove ${displayName}'s access to this property?`,
      header: 'Confirm Remove Access',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Remove',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: async () => {
        const success = await this.sharingService.revokeAccess(
          this.selectedProperty()!.id,
          collaborator.userId!
        );

        if (success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Access removed successfully.'
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.sharingService.error() || 'Failed to remove access.'
          });
        }
      }
    });
  }
}

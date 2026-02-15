import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { AccordionModule } from 'primeng/accordion';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-help',
  standalone: true,
  imports: [RouterLink, CardModule, AccordionModule, DividerModule],
  template: `
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Page Header -->
      <div class="mb-8">
        <h1 class="text-2xl md:text-3xl font-bold" style="color: var(--color-text-primary);">Getting Started</h1>
        <p style="color: var(--color-text-secondary);" class="mt-1">Everything you need to know to start billing</p>
      </div>

      <div class="flex flex-col gap-6">
        <!-- 1. Quick Start Card -->
        <p-card header="Quick Start">
          <div class="flex items-center gap-2 mb-4">
            <i class="pi pi-bolt text-lg" style="color: var(--color-accent);"></i>
            <span class="font-semibold text-lg" style="color: var(--color-text-primary);">Get up and running in 5 steps</span>
          </div>
          <div class="flex flex-col gap-4">
            <div class="flex items-start gap-3">
              <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0 text-sm font-bold text-white gradient-accent">1</div>
              <div>
                <span class="font-medium" style="color: var(--color-text-primary);">Sign in with Google</span>
                <p class="text-sm mt-0.5" style="color: var(--color-text-secondary);">Use your Google account to log in securely.</p>
              </div>
            </div>
            <div class="flex items-start gap-3">
              <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0 text-sm font-bold text-white gradient-accent">2</div>
              <div>
                <span class="font-medium" style="color: var(--color-text-primary);">Add a property</span>
                <p class="text-sm mt-0.5" style="color: var(--color-text-secondary);">Enter the property name and address on the <a routerLink="/properties" style="color: var(--color-accent);">Properties</a> page.</p>
              </div>
            </div>
            <div class="flex items-start gap-3">
              <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0 text-sm font-bold text-white gradient-accent">3</div>
              <div>
                <span class="font-medium" style="color: var(--color-text-primary);">Set default rates</span>
                <p class="text-sm mt-0.5" style="color: var(--color-text-secondary);">Configure electricity, water, and sanitation rates for the property.</p>
              </div>
            </div>
            <div class="flex items-start gap-3">
              <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0 text-sm font-bold text-white gradient-accent">4</div>
              <div>
                <span class="font-medium" style="color: var(--color-text-primary);">Create a bill</span>
                <p class="text-sm mt-0.5" style="color: var(--color-text-secondary);">Select a property, enter meter readings, and review the cost preview on the <a routerLink="/bills/create" style="color: var(--color-accent);">Create Bill</a> page.</p>
              </div>
            </div>
            <div class="flex items-start gap-3">
              <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0 text-sm font-bold text-white gradient-accent">5</div>
              <div>
                <span class="font-medium" style="color: var(--color-text-primary);">Download the PDF invoice</span>
                <p class="text-sm mt-0.5" style="color: var(--color-text-secondary);">View the bill and download a professional PDF to share with your tenant.</p>
              </div>
            </div>
          </div>
        </p-card>

        <!-- 2. How Billing Works Card -->
        <p-card header="How Billing Works">
          <div class="flex flex-col gap-4">
            <p style="color: var(--color-text-secondary);">
              FlatRate calculates utility bills using three components: electricity, water, and sanitation. Each has its own rate structure.
            </p>

            <div class="flex flex-col gap-3">
              <div>
                <h4 class="font-semibold" style="color: var(--color-text-primary);">
                  <i class="pi pi-bolt mr-2" style="color: var(--color-accent);"></i>Electricity
                </h4>
                <p class="text-sm ml-7" style="color: var(--color-text-secondary);">Flat rate per kWh. Formula: Units x Rate</p>
              </div>
              <div>
                <h4 class="font-semibold" style="color: var(--color-text-primary);">
                  <i class="pi pi-inbox mr-2" style="color: var(--color-accent);"></i>Water
                </h4>
                <p class="text-sm ml-7" style="color: var(--color-text-secondary);">3-tier sliding scale (0-6 kL, 7-15 kL, 16+ kL). Each tier has its own rate. Usage fills tiers sequentially.</p>
              </div>
              <div>
                <h4 class="font-semibold" style="color: var(--color-text-primary);">
                  <i class="pi pi-inbox mr-2" style="color: var(--color-accent);"></i>Sanitation
                </h4>
                <p class="text-sm ml-7" style="color: var(--color-text-secondary);">Same 3-tier structure as water, with separate rates.</p>
              </div>
              <div>
                <h4 class="font-semibold" style="color: var(--color-text-primary);">
                  <i class="pi pi-percentage mr-2" style="color: var(--color-accent);"></i>VAT
                </h4>
                <p class="text-sm ml-7" style="color: var(--color-text-secondary);">15% applied to the subtotal of all utilities. Total = Subtotal + VAT.</p>
              </div>
            </div>

            <p-divider />

            <!-- Worked Example -->
            <div class="rounded-lg p-4" style="background: var(--color-bg-tertiary);">
              <h4 class="font-semibold mb-3" style="color: var(--color-text-primary);">
                <i class="pi pi-calculator mr-2" style="color: var(--color-accent);"></i>Worked Example
              </h4>
              <p class="text-sm mb-3" style="color: var(--color-text-secondary);">
                A property uses 150 kWh of electricity at R 2.50/kWh, 20 kL of water, and 18 kL of sanitation.
              </p>
              <div class="flex flex-col gap-1 text-sm font-mono">
                <div class="flex justify-between" style="color: var(--color-text-secondary);">
                  <span>Electricity: 150 x R 2.50</span>
                  <span style="color: var(--color-text-primary);">R 375.00</span>
                </div>
                <div class="flex justify-between" style="color: var(--color-text-secondary);">
                  <span>Water: (6 x R 10.00) + (9 x R 15.00) + (5 x R 20.00)</span>
                  <span style="color: var(--color-text-primary);">R 295.00</span>
                </div>
                <div class="flex justify-between" style="color: var(--color-text-secondary);">
                  <span>Sanitation: (6 x R 8.00) + (9 x R 12.00) + (3 x R 16.00)</span>
                  <span style="color: var(--color-text-primary);">R 204.00</span>
                </div>
                <p-divider />
                <div class="flex justify-between" style="color: var(--color-text-secondary);">
                  <span>Subtotal</span>
                  <span style="color: var(--color-text-primary);">R 874.00</span>
                </div>
                <div class="flex justify-between" style="color: var(--color-text-secondary);">
                  <span>VAT (15%)</span>
                  <span style="color: var(--color-text-primary);">R 131.10</span>
                </div>
                <div class="flex justify-between font-bold" style="color: var(--color-accent);">
                  <span>Total</span>
                  <span>R 1,005.10</span>
                </div>
              </div>
            </div>
          </div>
        </p-card>

        <!-- 3. Water & Sanitation Tiers Card -->
        <p-card header="Water &amp; Sanitation Tiers">
          <p class="mb-4" style="color: var(--color-text-secondary);">
            Both water and sanitation use a 3-tier sliding scale. Usage fills each tier sequentially — you only pay the higher rate on the portion that exceeds the previous tier's limit.
          </p>

          <div class="overflow-x-auto">
            <table class="w-full text-sm" style="border-collapse: collapse;">
              <thead>
                <tr style="border-bottom: 2px solid var(--color-border);">
                  <th class="text-left py-2 pr-4 font-semibold" style="color: var(--color-text-primary);">Tier</th>
                  <th class="text-left py-2 pr-4 font-semibold" style="color: var(--color-text-primary);">Range</th>
                  <th class="text-left py-2 font-semibold" style="color: var(--color-text-primary);">Description</th>
                </tr>
              </thead>
              <tbody>
                <tr style="border-bottom: 1px solid var(--color-border);">
                  <td class="py-3 pr-4 font-medium" style="color: var(--color-text-primary);">Tier 1</td>
                  <td class="py-3 pr-4" style="color: var(--color-text-secondary);">0 - 6 kL</td>
                  <td class="py-3" style="color: var(--color-text-secondary);">First 6 kilolitres at the base rate</td>
                </tr>
                <tr style="border-bottom: 1px solid var(--color-border);">
                  <td class="py-3 pr-4 font-medium" style="color: var(--color-text-primary);">Tier 2</td>
                  <td class="py-3 pr-4" style="color: var(--color-text-secondary);">7 - 15 kL</td>
                  <td class="py-3" style="color: var(--color-text-secondary);">Next 9 kilolitres at a higher rate</td>
                </tr>
                <tr>
                  <td class="py-3 pr-4 font-medium" style="color: var(--color-text-primary);">Tier 3</td>
                  <td class="py-3 pr-4" style="color: var(--color-text-secondary);">16+ kL</td>
                  <td class="py-3" style="color: var(--color-text-secondary);">All remaining kilolitres at the highest rate</td>
                </tr>
              </tbody>
            </table>
          </div>

          <p-divider />

          <!-- Worked Example for Tiers -->
          <div class="rounded-lg p-4" style="background: var(--color-bg-tertiary);">
            <h4 class="font-semibold mb-3" style="color: var(--color-text-primary);">
              <i class="pi pi-calculator mr-2" style="color: var(--color-accent);"></i>Example: 20 kL of water
            </h4>
            <div class="flex flex-col gap-1 text-sm font-mono">
              <div class="flex justify-between" style="color: var(--color-text-secondary);">
                <span>Tier 1: 6 kL x R 10.00</span>
                <span style="color: var(--color-text-primary);">R 60.00</span>
              </div>
              <div class="flex justify-between" style="color: var(--color-text-secondary);">
                <span>Tier 2: 9 kL x R 15.00</span>
                <span style="color: var(--color-text-primary);">R 135.00</span>
              </div>
              <div class="flex justify-between" style="color: var(--color-text-secondary);">
                <span>Tier 3: 5 kL x R 20.00</span>
                <span style="color: var(--color-text-primary);">R 100.00</span>
              </div>
              <p-divider />
              <div class="flex justify-between font-bold" style="color: var(--color-accent);">
                <span>Total</span>
                <span>R 295.00 for 20 kL</span>
              </div>
            </div>
          </div>

          <p class="text-sm mt-4" style="color: var(--color-text-muted);">
            <i class="pi pi-info-circle mr-1"></i>
            Sanitation works identically but with its own set of rates.
          </p>
        </p-card>

        <!-- 4. Sharing Properties Card -->
        <p-card header="Sharing Properties">
          <p class="mb-4" style="color: var(--color-text-secondary);">
            FlatRate lets you share property access with other users. This is useful when a property manager or co-owner needs to create bills.
          </p>

          <!-- Roles -->
          <h4 class="font-semibold mb-3" style="color: var(--color-text-primary);">Roles</h4>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
            <div class="rounded-lg p-4 border" style="background: var(--color-bg-tertiary); border-color: var(--color-border);">
              <div class="flex items-center gap-2 mb-2">
                <i class="pi pi-shield" style="color: var(--color-accent);"></i>
                <span class="font-semibold" style="color: var(--color-text-primary);">Owner</span>
              </div>
              <ul class="text-sm list-disc list-inside flex flex-col gap-1" style="color: var(--color-text-secondary);">
                <li>Edit property details</li>
                <li>Set rates</li>
                <li>Create bills</li>
                <li>Manage collaborators</li>
                <li>Delete the property</li>
              </ul>
              <p class="text-xs mt-2" style="color: var(--color-text-muted);">
                The person who created the property is always the Owner.
              </p>
            </div>
            <div class="rounded-lg p-4 border" style="background: var(--color-bg-tertiary); border-color: var(--color-border);">
              <div class="flex items-center gap-2 mb-2">
                <i class="pi pi-pencil" style="color: var(--color-accent);"></i>
                <span class="font-semibold" style="color: var(--color-text-primary);">Editor</span>
              </div>
              <ul class="text-sm list-disc list-inside flex flex-col gap-1" style="color: var(--color-text-secondary);">
                <li>View the property</li>
                <li>Edit property details</li>
                <li>Set rates</li>
                <li>Create bills</li>
              </ul>
              <p class="text-xs mt-2" style="color: var(--color-text-muted);">
                Cannot delete the property or manage sharing access.
              </p>
            </div>
          </div>

          <p-divider />

          <!-- How to Share -->
          <h4 class="font-semibold mb-3 mt-2" style="color: var(--color-text-primary);">How to share a property</h4>
          <ol class="text-sm list-decimal list-inside flex flex-col gap-2 mb-6" style="color: var(--color-text-secondary);">
            <li>Go to the <a routerLink="/properties" style="color: var(--color-accent);">Properties</a> page</li>
            <li>Click the Share button (people icon) on the property you want to share</li>
            <li>Enter the email address of the person you want to invite</li>
            <li>Click "Invite" -- they will appear as "Pending" until they sign in</li>
            <li>Once they sign in with that email, they will see the shared property in their list</li>
          </ol>

          <!-- How to Remove Access -->
          <h4 class="font-semibold mb-3" style="color: var(--color-text-primary);">How to remove access</h4>
          <ol class="text-sm list-decimal list-inside flex flex-col gap-2" style="color: var(--color-text-secondary);">
            <li>Open the Share dialog for the property</li>
            <li>Click the x button next to the collaborator you want to remove</li>
            <li>Confirm the removal</li>
          </ol>

          <p class="text-sm mt-4" style="color: var(--color-text-muted);">
            <i class="pi pi-info-circle mr-1"></i>
            You can only manage sharing if you are the Owner of the property. Shared properties appear in your list with a "Shared" badge.
          </p>
        </p-card>

        <!-- 5. FAQ Section -->
        <div>
          <h2 class="text-xl font-bold mb-4" style="color: var(--color-text-primary);">
            Frequently Asked Questions
          </h2>
          <p-accordion [multiple]="true">
            <p-accordion-panel>
              <p-accordion-header>Can I edit a bill after creating it?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  No. Bills are immutable once created. If you need to correct a bill, delete it
                  and create a new one with the correct readings and rates.
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>How do I change rates?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  You can set default rates on each property (Properties &rarr; Set Rates). These rates
                  are automatically pre-filled when creating a new bill. You can also override the
                  rates on a per-bill basis during bill creation.
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>What currency does FlatRate use?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  South African Rand (ZAR). All amounts are displayed with the "R" prefix (e.g., R 150.00).
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>How do I share a property with someone else?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  Go to the Properties page and click the Share button (people icon) on the property.
                  Enter the person's email address and click "Invite". They will appear as "Pending" until
                  they sign in with that email. Once they do, the property will appear in their property list.
                  See the "Sharing Properties" section above for more detail on roles and permissions.
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>What is the difference between Owner and Editor?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  Owners have full control -- they can edit the property, manage rates, create bills,
                  invite or remove collaborators, and delete the property. Editors can do everything
                  except delete the property and manage sharing access.
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>How is VAT calculated?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  VAT is calculated at 15% (the South African standard rate) on the subtotal --
                  the sum of electricity, water, and sanitation costs. The formula is:
                  Total = Subtotal + (Subtotal x 0.15).
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>What do the water/sanitation tiers mean?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  Usage is split across three tiers. The first 6 kL is billed at Tier 1 rate.
                  The next 9 kL (7-15 kL) at Tier 2 rate. Anything above 15 kL at Tier 3 rate.
                  You only pay the higher rate on the portion that exceeds each tier's limit.
                </p>
              </p-accordion-content>
            </p-accordion-panel>
            <p-accordion-panel>
              <p-accordion-header>Can I download a bill as a PDF?</p-accordion-header>
              <p-accordion-content>
                <p style="color: var(--color-text-secondary);">
                  Yes. Go to the Bills page, click the eye icon to view a bill's details, then click
                  "Download PDF" at the bottom of the dialog. The file will be named with the invoice number.
                </p>
              </p-accordion-content>
            </p-accordion-panel>
          </p-accordion>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HelpPage {}

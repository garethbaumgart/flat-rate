import { Routes } from '@angular/router';
import { HomePage } from './home/home.page';
import { PropertiesPage } from './properties/properties.page';
import { BillsPage } from './bills/bills.page';
import { CreateBillPage } from './bills/create-bill.page';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'properties', component: PropertiesPage, canActivate: [authGuard] },
  { path: 'bills', component: BillsPage, canActivate: [authGuard] },
  { path: 'bills/create', component: CreateBillPage, canActivate: [authGuard] }
];

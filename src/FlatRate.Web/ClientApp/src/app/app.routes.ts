import { Routes } from '@angular/router';
import { HomePage } from './home/home.page';
import { PropertiesPage } from './properties/properties.page';
import { BillsPage } from './bills/bills.page';
import { CreateBillPage } from './bills/create-bill.page';

export const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'properties', component: PropertiesPage },
  { path: 'bills', component: BillsPage },
  { path: 'bills/create', component: CreateBillPage }
];

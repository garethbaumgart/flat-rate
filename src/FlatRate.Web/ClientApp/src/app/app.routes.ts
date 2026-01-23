import { Routes } from '@angular/router';
import { HomePage } from './home/home.page';
import { PropertiesPage } from './properties/properties.page';
import { CreateBillPage } from './bills/create-bill.page';

export const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'properties', component: PropertiesPage },
  { path: 'bills/create', component: CreateBillPage },
  { path: 'bills', redirectTo: 'bills/create', pathMatch: 'full' }
];

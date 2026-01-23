import { Routes } from '@angular/router';
import { HomePage } from './home/home.page';
import { PropertiesPage } from './properties/properties.page';
import { CreateBillPage } from './bills/create-bill.page';

export const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'properties', component: PropertiesPage },
  { path: 'bills/create', component: CreateBillPage }
  // Bill history route will be added in Issue #7
];

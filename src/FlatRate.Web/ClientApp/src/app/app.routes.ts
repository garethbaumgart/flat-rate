import { Routes } from '@angular/router';
import { HomePage } from './home/home.page';
import { PropertiesPage } from './properties/properties.page';

export const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'properties', component: PropertiesPage }
];

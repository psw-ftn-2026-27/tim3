import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourDetails } from './tour-browsing/tour-details/tour-details';
import { TourList } from './tour-browsing/tour-list/tour-list';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'create', component: CreateTour },
  { path: ':id', component: TourDetails },
];

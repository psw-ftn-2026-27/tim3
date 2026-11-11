import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { MyTourPreference } from './tour-preference-management/my-tour-preference/my-tour-preference';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'create', component: CreateTour },
  { path: 'preferences', component: MyTourPreference },
];

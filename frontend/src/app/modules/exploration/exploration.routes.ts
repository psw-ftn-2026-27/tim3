import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { KeyPoints } from './tour-authoring/key-points/key-points';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourList } from './tour-browsing/tour-list/tour-list';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id/key-points', component: KeyPoints },
  { path: 'create', component: CreateTour },
];

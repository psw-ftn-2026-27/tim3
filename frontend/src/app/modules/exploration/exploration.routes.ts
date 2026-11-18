import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { KeyPoints } from './tour-authoring/key-points/key-points';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { PurchasedTourDetails } from './tour-browsing/purchased-tour-details/purchased-tour-details';
import { PurchasedTours } from './tour-browsing/purchased-tours/purchased-tours';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { TourPreview } from './tour-browsing/tour-preview/tour-preview';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id/key-points', component: KeyPoints },
  { path: 'create', component: CreateTour },
  { path: 'purchased', component: PurchasedTours },
  { path: 'purchased/:id', component: PurchasedTourDetails },
  { path: 'tours/:id', component: TourPreview },
  { path: ':id', component: TourPreview },
];

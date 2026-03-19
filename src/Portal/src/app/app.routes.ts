import { Routes } from '@angular/router';
import { LandingComponent } from './pages/landing-component/landing-component';
import { GameDetailsComponent } from './pages/game-details-component/game-details-component';

export const routes: Routes = [
  { path: '', component: LandingComponent, title: 'IS IT WORTH IT?' },
  { path: 'game/:id', component: GameDetailsComponent, title: 'IS IT WORTH IT?' },
  { path: '**', redirectTo: '' },
];

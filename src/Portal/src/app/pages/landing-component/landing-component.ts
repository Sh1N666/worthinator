import { Component, inject, signal, Signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { distinctUntilChanged, finalize, map, of, startWith, switchMap, tap, timer } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { WorthinatorService } from '../../services/worthinator-service';
import { SearchSuggestions } from '../../components/search-suggestions/search-suggestions';
import { GamePreview } from '../../generated/worthinator';
import { Router } from '@angular/router';

@Component({
  selector: 'app-landing-component',
  standalone: true,
  imports: [ReactiveFormsModule, SearchSuggestions],
  templateUrl: './landing-component.html',
})
export class LandingComponent {
  private readonly worthinatorService = inject(WorthinatorService);
  private router = inject(Router);
  searchControl = new FormControl('', { nonNullable: true });
  isLoading = signal(false);

  foundGames: Signal<GamePreview[]> = toSignal(
    this.searchControl.valueChanges.pipe(
      distinctUntilChanged(),
      tap((phrase) => {
        if (phrase) this.isLoading.set(true);
      }),
      switchMap((phrase) => {
        if (!phrase) {
          this.isLoading.set(false);
          return of([]);
        }
        return timer(300).pipe(
          switchMap(() =>
            this.worthinatorService.searchGames(phrase).pipe(
              map((results) => results.slice(0, 5)),
              finalize(() => this.isLoading.set(false)),
            ),
          ),
          startWith([]),
        );
      }),
    ),
    { initialValue: [] },
  );

  onGameClick(game: GamePreview) {
    this.searchControl.setValue('');
    this.router.navigate(['/game', game.steamAppId]);
  }
}

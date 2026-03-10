import { Component, computed } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';

interface GameMock {
  id: number;
  title: string;
  price: string;
  worthIt: boolean;
}

@Component({
  selector: 'app-landing-component',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './landing-component.html',
})
export class LandingComponent {
  searchControl = new FormControl('', { nonNullable: true });

  searchQuery = toSignal(
    this.searchControl.valueChanges.pipe(debounceTime(200), distinctUntilChanged()),
    { initialValue: '' },
  );

  allResults = computed<GameMock[]>(() => {
    const q = this.searchQuery().toLowerCase();
    if (q.length < 2) return [];

    const mockDatabase: GameMock[] = [
      { id: 1, title: 'The Witcher 3', price: '29.99', worthIt: true },
      { id: 2, title: 'The Witcher 2', price: '9.00', worthIt: true },
      { id: 3, title: 'The Witcher 1', price: '5.00', worthIt: true },
      { id: 4, title: 'The Witcher 4: Polaris', price: '299.00', worthIt: false },
      { id: 5, title: 'The Witcher: Sirius', price: '199.00', worthIt: false },
      { id: 6, title: 'The Witcher Remake', price: '249.00', worthIt: false },
      { id: 7, title: 'The Witcher Adventure Game', price: '30.00', worthIt: true },
      { id: 8, title: 'The Witcher Battle Arena', price: '0.00', worthIt: false },
    ];

    return mockDatabase.filter((game) => game.title.toLowerCase().includes(q));
  });

  filteredResults = computed(() => this.allResults().slice(0, 5));
}

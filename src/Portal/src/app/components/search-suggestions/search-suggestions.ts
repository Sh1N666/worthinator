import { NgOptimizedImage } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { GamePreview } from '../../generated/worthinator';

@Component({
  selector: 'app-search-suggestions',
  imports: [NgOptimizedImage],
  templateUrl: './search-suggestions.html',
  styleUrl: './search-suggestions.css',
})
export class SearchSuggestions {
  games = input.required<GamePreview[]>();
  isLoading = input.required<boolean>();
  gameSelected = output<GamePreview>();
}

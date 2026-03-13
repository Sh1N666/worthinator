import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchSuggestions } from './search-suggestions';

describe('SearchSuggestions', () => {
  let component: SearchSuggestions;
  let fixture: ComponentFixture<SearchSuggestions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchSuggestions]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SearchSuggestions);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

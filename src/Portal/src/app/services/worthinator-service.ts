import { Injectable } from '@angular/core';
import { Observable, from, map } from 'rxjs';
import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import { WorthinatorServiceClient } from '../generated/worthinator.client';
import { SearchGameRequest } from '../generated/worthinator';

@Injectable({
  providedIn: 'root',
})
export class WorthinatorService {
  private transport = new GrpcWebFetchTransport({
    baseUrl: 'https://localhost:443/api',
    format: 'text',
  });

  private client = new WorthinatorServiceClient(this.transport);

  searchGames(phrase: string): Observable<any[]> {
    const request: SearchGameRequest = { searchPhrase: phrase };
    return from(this.client.searchGame(request)).pipe(map((call) => call.response.games));
  }
}

import { Injectable } from '@angular/core';
import { Observable, delay, from, map, of } from 'rxjs';
import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import { WorthinatorServiceClient } from '../generated/worthinator.client';
import {
  GamePreview,
  GetGameInfoRequest,
  GetGameInfoResponse,
  SearchGameRequest,
} from '../generated/worthinator';

@Injectable({
  providedIn: 'root',
})
export class WorthinatorService {
  private transport = new GrpcWebFetchTransport({
    baseUrl: 'https://localhost:443/api',
    format: 'text',
  });

  private client = new WorthinatorServiceClient(this.transport);

  searchGames(phrase: string): Observable<GamePreview[]> {
    const request: SearchGameRequest = { searchPhrase: phrase };
    return from(this.client.searchGame(request)).pipe(map((call) => call.response.games));
  }

  getGameInfo(id: number): Observable<GetGameInfoResponse> {
    const request: GetGameInfoRequest = { steamAppId: id };
    return from(this.client.getGameInfo(request)).pipe(map((call) => call.response));
  }
}

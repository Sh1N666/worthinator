export interface SearchGameResponse {
  steamapp_id: string;
  name: string;
  tiny_image: string;
}

export interface HowLongToBeat {
  main: number;
  main_extra: number;
  completionist: number;
}

export interface PriceInfo {
  actual_price: number;
  historical_low_price: number;
  initial_price: number;
  currency_code: string;
}

export interface GetGameInfoResponse {
  id: string;
  worth_factor: number;
  price_info: PriceInfo;
  tiny_image: string;
  rating_score: number;
  release_year: number;
  how_long_to_beat: HowLongToBeat;
}

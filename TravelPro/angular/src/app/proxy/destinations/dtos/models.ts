import type { Coordinate } from '../models';

export interface CityDto {
  name?: string;
  country?: string;
  population: number;
  coordinates: Coordinate;
}

export interface SearchDestinationsInputDto {
  partialName?: string;
}

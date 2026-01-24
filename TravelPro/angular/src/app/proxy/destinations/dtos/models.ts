import type { AuditedEntityDto } from '@abp/ng.core';
import type { Coordinate } from '../models';

export interface CityDto extends AuditedEntityDto<string> {
  name?: string;
  country?: string;
  population: number;
  region?: string;
  coordinates: Coordinate;
}

export interface CountryDto {
  name?: string;
  code?: string;
}

export interface RegionDto {
  name?: string;
  code?: string;
}

export interface SearchDestinationsInputDto {
  partialName?: string;
  minPopulation?: number;
  country?: string;
  region?: string;
  countryName?: string;
  regionName?: string;
}

import type { AuditedEntityDto } from '@abp/ng.core';
import type { Coordinate } from '../models';

export interface CityDto extends AuditedEntityDto<string> {
  name?: string;
  country?: string;
  population: number;
  region?: string;
  coordinates: Coordinate;
}

export interface SearchDestinationsInputDto {
  partialName?: string;
}

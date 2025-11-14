import type { ValueObject } from '../volo/abp/domain/values/models';
import type { AuditedEntityDto } from '@abp/ng.core';

export interface Coordinate extends ValueObject {
  latitude?: string;
  longitude?: string;
}

export interface CreateUpdateDestinationDto {
  name: string;
  coordinates: Coordinate;
  lastUpdated: string;
  region: string;
  country: string;
  photo?: string;
  population: number;
}

export interface DestinationDto extends AuditedEntityDto<string> {
  name?: string;
  country?: string;
  population: number;
  photo?: string;
  region?: string;
  lastUpdated?: string;
  coordinates: Coordinate;
}

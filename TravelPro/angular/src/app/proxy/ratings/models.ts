import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateRatingDto {
  destinationId: string;
  userId: string;
  score: number;
  comment?: string;
}

export interface RatingDto extends AuditedEntityDto<string> {
  destinationId?: string;
  userId?: string;
  userName?: string;
  score: number;
  comment?: string;
}

export interface RatingStatsDto {
  averageScore: number;
  totalCount: number;
}

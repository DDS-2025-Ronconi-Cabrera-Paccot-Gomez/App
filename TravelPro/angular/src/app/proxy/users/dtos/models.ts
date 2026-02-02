import type { EntityDto } from '@abp/ng.core';

export interface PublicProfileDto extends EntityDto<string> {
  userName?: string;
  name?: string;
  surname?: string;
  profilePhoto?: string;
}

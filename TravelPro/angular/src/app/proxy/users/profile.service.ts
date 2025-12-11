import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ChangePasswordInput, ProfileDto, UpdateProfileDto } from '../volo/abp/account/models';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  apiName = 'Default';
  

  changePassword = (input: ChangePasswordInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/profile/change-password',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProfileDto>({
      method: 'GET',
      url: '/api/profile',
    },
    { apiName: this.apiName,...config });
  

  update = (input: UpdateProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProfileDto>({
      method: 'PUT',
      url: '/api/profile',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

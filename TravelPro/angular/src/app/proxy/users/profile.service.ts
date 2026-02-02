import type { PublicProfileDto } from './dtos/models';
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
  

  delete = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/profile',
    },
    { apiName: this.apiName,...config });
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProfileDto>({
      method: 'GET',
      url: '/api/profile',
    },
    { apiName: this.apiName,...config });
  

  getPublicProfile = (userId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicProfileDto>({
      method: 'GET',
      url: `/api/profile/public/${userId}`,
    },
    { apiName: this.apiName,...config });
  

  getPublicProfileByUserName = (userName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicProfileDto>({
      method: 'GET',
      url: `/api/profile/public/username/${userName}`,
    },
    { apiName: this.apiName,...config });
  

  search = (filter: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicProfileDto[]>({
      method: 'GET',
      url: '/api/profile/search',
      params: { filter },
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

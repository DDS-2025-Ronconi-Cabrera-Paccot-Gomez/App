import type { CityDto, CountryDto, RegionDto, SearchDestinationsInputDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CitySearchService {
  apiName = 'Default';
  

  getCountries = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CountryDto[]>({
      method: 'GET',
      url: '/api/app/city-search/countries',
    },
    { apiName: this.apiName,...config });
  

  getRegions = (countryCode: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RegionDto[]>({
      method: 'GET',
      url: '/api/app/city-search/regions',
      params: { countryCode },
    },
    { apiName: this.apiName,...config });
  

  searchCities = (input: SearchDestinationsInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CityDto>>({
      method: 'POST',
      url: '/api/app/city-search/search-cities',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

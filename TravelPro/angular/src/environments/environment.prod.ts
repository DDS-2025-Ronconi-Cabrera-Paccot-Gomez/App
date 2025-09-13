import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44380/',
  redirectUri: baseUrl,
  clientId: 'TravelPro_App',
  responseType: 'code',
  scope: 'offline_access TravelPro',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'TravelPro',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44380',
      rootNamespace: 'TravelPro',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;

import type { InternalAxiosRequestConfig } from 'axios';

export const requestInterceptor = (config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('auth_token');

  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  if (import.meta.env.DEV) {
    console.log('Request:', config.method?.toUpperCase(), config.url, config.data);
  }

  return config;
};

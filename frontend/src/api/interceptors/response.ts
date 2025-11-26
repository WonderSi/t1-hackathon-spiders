import type { AxiosResponse, AxiosError } from 'axios';

export const responseInterceptor = (response: AxiosResponse) => {
  if (import.meta.env.DEV) {
    console.log('Response:', response.config.url, response.data);
  }
  
  return response;
};

export const errorInterceptor = (error: AxiosError) => {
  if (import.meta.env.DEV) {
    console.error('Error:', error.config?.url, error.response?.data || error.message);
  }

  return Promise.reject(error);
};

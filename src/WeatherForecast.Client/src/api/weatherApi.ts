import axios from 'axios';
import type { WeatherDashboard } from '../types/weather';

const apiClient = axios.create({
  baseURL: '/api',
  timeout: 15000,
  headers: {
    'Accept': 'application/json',
  },
});

export async function fetchWeatherDashboard(): Promise<WeatherDashboard> {
  const response = await apiClient.get<WeatherDashboard>('/weather/dashboard');
  return response.data;
}

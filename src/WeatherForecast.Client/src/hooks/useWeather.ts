import { useState, useEffect, useCallback, useRef } from 'react';
import type { WeatherDashboard } from '../types/weather';
import { fetchWeatherDashboard } from '../api/weatherApi';

const AUTO_REFRESH_INTERVAL = 10 * 60 * 1000; // 10 minutes

interface UseWeatherResult {
  data: WeatherDashboard | null;
  isLoading: boolean;
  error: string | null;
  lastFetchedAt: Date | null;
  refresh: () => void;
}

export function useWeather(): UseWeatherResult {
  const [data, setData] = useState<WeatherDashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastFetchedAt, setLastFetchedAt] = useState<Date | null>(null);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await fetchWeatherDashboard();
      setData(result);
      setLastFetchedAt(new Date());
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch weather data';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();

    intervalRef.current = setInterval(fetchData, AUTO_REFRESH_INTERVAL);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [fetchData]);

  return { data, isLoading, error, lastFetchedAt, refresh: fetchData };
}

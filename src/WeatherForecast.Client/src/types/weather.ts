export interface WeatherDashboard {
  location: Location;
  current: CurrentWeather;
  hourlyForecast: HourForecast[];
  dailyForecast: DayForecast[];
}

export interface Location {
  name: string;
  region: string;
  country: string;
  timeZone: string;
  localTime: string;
}

export interface CurrentWeather {
  tempCelsius: number;
  feelsLikeCelsius: number;
  humidity: number;
  pressureMb: number;
  windSpeedKph: number;
  windDirection: string;
  cloudCover: number;
  uvIndex: number;
  visibilityKm: number;
  conditionText: string;
  conditionIconUrl: string;
  isDay: boolean;
  lastUpdated: string;
}

export interface HourForecast {
  time: string;
  tempCelsius: number;
  feelsLikeCelsius: number;
  conditionText: string;
  conditionIconUrl: string;
  windSpeedKph: number;
  humidity: number;
  chanceOfRain: number;
  isDay: boolean;
}

export interface DayForecast {
  date: string;
  maxTempCelsius: number;
  minTempCelsius: number;
  avgTempCelsius: number;
  maxWindSpeedKph: number;
  avgHumidity: number;
  chanceOfRain: number;
  totalPrecipitationMm: number;
  uvIndex: number;
  conditionText: string;
  conditionIconUrl: string;
}

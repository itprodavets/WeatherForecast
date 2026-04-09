import type { CurrentWeather as CurrentWeatherType } from '../types/weather';

interface CurrentWeatherProps {
  data: CurrentWeatherType;
}

export function CurrentWeather({ data }: CurrentWeatherProps) {
  return (
    <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 shadow-2xl">
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-3">
            <span className="text-6xl font-thin text-white">
              {Math.round(data.tempCelsius)}°
            </span>
            <img
              src={data.conditionIconUrl}
              alt={data.conditionText}
              className="w-16 h-16"
            />
          </div>
          <p className="text-blue-200 text-lg mt-1">{data.conditionText}</p>
          <p className="text-blue-300/60 text-sm mt-1">
            Feels like {Math.round(data.feelsLikeCelsius)}°
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
        <WeatherStat label="Humidity" value={`${data.humidity}%`} />
        <WeatherStat label="Wind" value={`${data.windSpeedKph} km/h ${data.windDirection}`} />
        <WeatherStat label="Pressure" value={`${data.pressureMb} mb`} />
        <WeatherStat label="UV Index" value={data.uvIndex.toString()} />
        <WeatherStat label="Visibility" value={`${data.visibilityKm} km`} />
        <WeatherStat label="Cloud Cover" value={`${data.cloudCover}%`} />
      </div>
    </div>
  );
}

function WeatherStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-white/5 rounded-xl p-3 text-center">
      <p className="text-blue-300/60 text-xs uppercase tracking-wider">{label}</p>
      <p className="text-white font-medium mt-1">{value}</p>
    </div>
  );
}

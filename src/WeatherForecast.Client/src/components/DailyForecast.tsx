import type { DayForecast } from '../types/weather';

interface DailyForecastProps {
  days: DayForecast[];
}

export function DailyForecast({ days }: DailyForecastProps) {
  return (
    <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 shadow-2xl">
      <h2 className="text-white font-semibold text-lg mb-4">3-Day Forecast</h2>
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {days.map((day, index) => (
          <DayCard key={day.date} day={day} isToday={index === 0} />
        ))}
      </div>
    </div>
  );
}

function DayCard({ day, isToday }: { day: DayForecast; isToday: boolean }) {
  const date = new Date(day.date + 'T00:00:00');
  const dayName = isToday
    ? 'Today'
    : date.toLocaleDateString('en-US', { weekday: 'short' });
  const dateStr = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });

  return (
    <div className="bg-white/5 rounded-xl p-4 hover:bg-white/10 transition-colors">
      <div className="flex items-center justify-between mb-3">
        <div>
          <p className="text-white font-medium">{dayName}</p>
          <p className="text-blue-300/60 text-xs">{dateStr}</p>
        </div>
        <img
          src={day.conditionIconUrl}
          alt={day.conditionText}
          className="w-12 h-12"
        />
      </div>

      <p className="text-blue-200 text-sm mb-3">{day.conditionText}</p>

      <div className="flex items-center justify-between">
        <span className="text-white font-semibold">
          {Math.round(day.maxTempCelsius)}°
        </span>
        <div className="flex-1 mx-3 h-1.5 bg-gradient-to-r from-blue-400 to-orange-400 rounded-full opacity-40" />
        <span className="text-blue-300/70">
          {Math.round(day.minTempCelsius)}°
        </span>
      </div>

      <div className="grid grid-cols-2 gap-2 mt-3 text-xs">
        <div className="text-blue-300/60">
          💧 Rain: <span className="text-blue-200">{day.chanceOfRain}%</span>
        </div>
        <div className="text-blue-300/60">
          💨 Wind: <span className="text-blue-200">{Math.round(day.maxWindSpeedKph)} km/h</span>
        </div>
        <div className="text-blue-300/60">
          💦 Humidity: <span className="text-blue-200">{day.avgHumidity}%</span>
        </div>
        <div className="text-blue-300/60">
          ☀️ UV: <span className="text-blue-200">{day.uvIndex}</span>
        </div>
      </div>
    </div>
  );
}

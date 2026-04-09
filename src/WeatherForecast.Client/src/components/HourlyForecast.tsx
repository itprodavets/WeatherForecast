import type { HourForecast } from '../types/weather';

interface HourlyForecastProps {
  hours: HourForecast[];
}

export function HourlyForecast({ hours }: HourlyForecastProps) {
  return (
    <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 shadow-2xl">
      <h2 className="text-white font-semibold text-lg mb-4">Hourly Forecast</h2>
      <div className="flex gap-3 overflow-x-auto pb-2 scrollbar-thin">
        {hours.map((hour, index) => (
          <HourCard key={index} hour={hour} />
        ))}
      </div>
    </div>
  );
}

function HourCard({ hour }: { hour: HourForecast }) {
  const time = new Date(hour.time);
  const timeStr = time.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });

  return (
    <div className="flex flex-col items-center min-w-[72px] bg-white/5 rounded-xl p-3 hover:bg-white/10 transition-colors">
      <span className="text-blue-300/70 text-xs">{timeStr}</span>
      <img
        src={hour.conditionIconUrl}
        alt={hour.conditionText}
        className="w-10 h-10 my-1"
      />
      <span className="text-white font-medium">{Math.round(hour.tempCelsius)}°</span>
      {hour.chanceOfRain > 0 && (
        <span className="text-blue-400 text-xs mt-1">
          💧 {hour.chanceOfRain}%
        </span>
      )}
    </div>
  );
}

import { useWeather } from './hooks/useWeather';
import { CurrentWeather } from './components/CurrentWeather';
import { HourlyForecast } from './components/HourlyForecast';
import { DailyForecast } from './components/DailyForecast';
import { LoadingSpinner } from './components/LoadingSpinner';
import { ErrorDisplay } from './components/ErrorDisplay';

function App() {
  const { data, isLoading, error, lastFetchedAt, refresh } = useWeather();

  if (isLoading && !data) {
    return <LoadingSpinner />;
  }

  if (error && !data) {
    return <ErrorDisplay message={error} onRetry={refresh} />;
  }

  if (!data) return null;

  const localTime = new Date(data.location.localTime);
  const formattedDate = localTime.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
  const formattedTime = localTime.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
  });

  const updatedAgo = lastFetchedAt
    ? getTimeAgo(lastFetchedAt)
    : '';

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-indigo-950 px-4 py-8">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Header */}
        <header className="text-center mb-8">
          <h1 className="text-4xl font-bold text-white tracking-tight">
            {data.location.name}
          </h1>
          <p className="text-blue-200/70 mt-1">
            {data.location.region}, {data.location.country}
          </p>
          <p className="text-blue-300/50 text-sm mt-1">
            {formattedDate} &bull; {formattedTime}
          </p>

          {/* Refresh indicator */}
          <div className="flex items-center justify-center gap-2 mt-3">
            {updatedAgo && (
              <span className="text-blue-400/50 text-xs">
                Updated {updatedAgo}
              </span>
            )}
            <button
              onClick={refresh}
              disabled={isLoading}
              className="text-blue-400/70 hover:text-blue-300 text-xs flex items-center gap-1 transition-colors disabled:opacity-50 cursor-pointer"
              title="Refresh weather data"
            >
              <svg
                className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`}
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2}
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                />
              </svg>
              Refresh
            </button>
          </div>
        </header>

        {/* Error banner (when data exists but refresh failed) */}
        {error && data && (
          <div className="bg-red-500/10 border border-red-500/20 rounded-xl p-3 text-center">
            <p className="text-red-300 text-sm">
              Failed to refresh: {error}
              <button
                onClick={refresh}
                className="ml-2 text-red-200 underline hover:text-white cursor-pointer"
              >
                Retry
              </button>
            </p>
          </div>
        )}

        <CurrentWeather data={data.current} />
        <HourlyForecast hours={data.hourlyForecast} />
        <DailyForecast days={data.dailyForecast} />
      </div>
    </div>
  );
}

function getTimeAgo(date: Date): string {
  const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes === 1) return '1 min ago';
  if (minutes < 60) return `${minutes} min ago`;
  const hours = Math.floor(minutes / 60);
  return `${hours}h ago`;
}

export default App;

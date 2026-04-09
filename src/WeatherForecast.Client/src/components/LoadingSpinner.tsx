export function LoadingSpinner() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-indigo-950">
      <div className="relative">
        <div className="w-16 h-16 border-4 border-blue-400/30 border-t-blue-400 rounded-full animate-spin" />
      </div>
      <p className="mt-6 text-blue-200 text-lg font-light animate-pulse">
        Loading weather data...
      </p>
    </div>
  );
}

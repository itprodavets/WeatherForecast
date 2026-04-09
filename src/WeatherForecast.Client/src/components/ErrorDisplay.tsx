interface ErrorDisplayProps {
  message: string;
  onRetry: () => void;
}

export function ErrorDisplay({ message, onRetry }: ErrorDisplayProps) {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-indigo-950">
      <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8 max-w-md text-center shadow-2xl">
        <div className="text-5xl mb-4">⚠️</div>
        <h2 className="text-xl font-semibold text-white mb-2">
          Something went wrong
        </h2>
        <p className="text-blue-200/70 mb-6 text-sm">
          {message}
        </p>
        <button
          onClick={onRetry}
          className="px-6 py-2.5 bg-blue-500 hover:bg-blue-400 text-white rounded-xl font-medium transition-all duration-200 hover:shadow-lg hover:shadow-blue-500/25 active:scale-95 cursor-pointer"
        >
          Try Again
        </button>
      </div>
    </div>
  );
}

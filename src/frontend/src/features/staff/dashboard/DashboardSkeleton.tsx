/**
 * Dashboard Skeleton Component for US_068 - Staff Dashboard
 * Loading state with skeleton placeholders (UXR-502)
 */

export function DashboardSkeleton() {
  return (
    <div className="animate-pulse">
      {/* Stat Cards Skeleton */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        {[1, 2, 3].map((i) => (
          <div
            key={i}
            className="bg-neutral-100 border border-neutral-200 rounded-lg p-6 h-32"
          >
            <div className="h-4 bg-neutral-200 rounded w-2/3 mb-3"></div>
            <div className="h-8 bg-neutral-200 rounded w-1/3"></div>
          </div>
        ))}
      </div>

      {/* Quick Actions Skeleton */}
      <div className="mb-6">
        <div className="h-6 bg-neutral-200 rounded w-32 mb-3"></div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <div
              key={i}
              className="bg-neutral-100 border border-neutral-200 rounded-lg p-4 h-20"
            ></div>
          ))}
        </div>
      </div>

      {/* Queue Preview Skeleton */}
      <div className="bg-neutral-100 border border-neutral-200 rounded-lg p-6">
        <div className="h-6 bg-neutral-200 rounded w-48 mb-4"></div>
        <div className="space-y-3">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-12 bg-neutral-200 rounded"></div>
          ))}
        </div>
      </div>
    </div>
  );
}

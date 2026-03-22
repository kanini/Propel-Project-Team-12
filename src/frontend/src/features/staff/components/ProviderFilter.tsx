/**
 * ProviderFilter Component for US_030 - Queue Management.
 * Dropdown filter for provider selection with "All Providers" default.
 */

import { useState, useEffect } from "react";

interface Provider {
  id: string;
  name: string;
  specialty: string;
}

interface ProviderFilterProps {
  /**
   * Selected provider ID (empty string for "All Providers")
   */
  value: string;
  /**
   * Callback when provider selection changes
   */
  onChange: (providerId: string) => void;
}

/**
 * Provider dropdown filter component
 */
export function ProviderFilter({ value, onChange }: ProviderFilterProps) {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetch providers list on mount
   */
  useEffect(() => {
    const fetchProviders = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const token = localStorage.getItem("token");
        const response = await fetch(
          `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/providers?page=1&pageSize=100`,
          {
            headers: {
              "Content-Type": "application/json",
              ...(token && { Authorization: `Bearer ${token}` }),
            },
          },
        );

        if (!response.ok) {
          throw new Error("Failed to fetch providers");
        }

        const data = await response.json();
        setProviders(data.providers || []);
      } catch (err) {
        console.error("Error fetching providers:", err);
        setError("Failed to load providers");
      } finally {
        setIsLoading(false);
      }
    };

    fetchProviders();
  }, []);

  return (
    <div className="w-full sm:w-64">
      <label
        htmlFor="provider-filter"
        className="block text-sm font-medium text-neutral-900 mb-2"
      >
        Filter by Provider
      </label>
      {isLoading ? (
        <div className="h-11 bg-neutral-200 rounded-lg animate-pulse" />
      ) : (
        <select
          id="provider-filter"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="w-full h-11 px-3 border border-neutral-300 rounded-lg text-sm text-neutral-900
                        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
                        bg-neutral-0 transition-colors"
          aria-label="Filter queue by provider"
        >
          <option value="">All Providers</option>
          {providers.map((provider) => (
            <option key={provider.id} value={provider.id}>
              {provider.name} - {provider.specialty}
            </option>
          ))}
        </select>
      )}
      {error && (
        <p className="mt-1 text-sm text-error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}

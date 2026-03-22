/**
 * usePatientSearch Custom Hook for US_032
 * Handles patient search logic with debounce and API integration
 */

import { useState, useEffect, useRef } from "react";
import type { PatientSearchResult } from "../components/shared/PatientSearch/types";

interface UsePatientSearchResult {
  results: PatientSearchResult[];
  isLoading: boolean;
  error: string | null;
  clearResults: () => void;
}

/**
 * Custom hook for patient search with 300ms debounce
 * @param query - Search query string
 * @returns Search results, loading state, error, and clear function
 */
export function usePatientSearch(query: string): UsePatientSearchResult {
  const [results, setResults] = useState<PatientSearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const debounceTimerRef = useRef<number | null>(null);

  /**
   * Clear search results
   */
  const clearResults = () => {
    setResults([]);
    setError(null);
    setIsLoading(false);
  };

  /**
   * Perform search API call
   */
  const performSearch = async (searchQuery: string) => {
    if (!searchQuery || searchQuery.trim().length < 2) {
      clearResults();
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem("token");
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/staff/patients/search?query=${encodeURIComponent(searchQuery)}`,
        {
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
        },
      );

      if (!response.ok) {
        throw new Error("Failed to search patients");
      }

      const data = await response.json();
      setResults(data);
    } catch (err) {
      console.error("Error searching patients:", err);
      setError(
        err instanceof Error ? err.message : "Failed to search patients",
      );
      setResults([]);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Debounced search effect (300ms)
   */
  useEffect(() => {
    // Clear existing timer
    if (debounceTimerRef.current !== null) {
      window.clearTimeout(debounceTimerRef.current);
    }

    // Return early if query is empty or too short
    if (!query || query.trim().length === 0) {
      clearResults();
      return;
    }

    if (query.trim().length < 2) {
      setResults([]);
      setIsLoading(false);
      return;
    }

    // Set debounce timer (300ms)
    debounceTimerRef.current = window.setTimeout(() => {
      performSearch(query);
    }, 300);

    // Cleanup on unmount or query change
    return () => {
      if (debounceTimerRef.current !== null) {
        window.clearTimeout(debounceTimerRef.current);
      }
    };
  }, [query]);

  return { results, isLoading, error, clearResults };
}

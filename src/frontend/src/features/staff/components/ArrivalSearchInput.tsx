/**
 * ArrivalSearchInput Component for US_031 - Patient Search with Today's Appointments
 * Implements 300ms debounce with keyboard navigation
 */

import { useState, useEffect, useRef } from "react";
import type { ArrivalAppointment } from "../../../types/arrival";

interface ArrivalSearchInputProps {
  onSelectAppointment: (appointment: ArrivalAppointment) => void;
  onNoAppointmentFound: (query: string) => void;
}

/**
 * Patient search input with today's appointment filtering
 */
export function ArrivalSearchInput({
  onSelectAppointment,
  onNoAppointmentFound,
}: ArrivalSearchInputProps) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<ArrivalAppointment[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [showResults, setShowResults] = useState(false);
  const [noResults, setNoResults] = useState(false);
  const debounceTimerRef = useRef<number | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  /**
   * Search for patients with today's appointments
   */
  const searchAppointments = async (searchQuery: string) => {
    if (!searchQuery || searchQuery.trim().length < 2) {
      setResults([]);
      setShowResults(false);
      setNoResults(false);
      return;
    }

    setIsSearching(true);
    setNoResults(false);

    try {
      const token = localStorage.getItem("token");
      const today = new Date().toISOString().split("T")[0]; // YYYY-MM-DD format
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/staff/arrivals/search?query=${encodeURIComponent(searchQuery)}&date=${today}`,
        {
          headers: {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
          },
        },
      );

      if (!response.ok) {
        throw new Error("Failed to search appointments");
      }

      const data = await response.json();
      setResults(data);
      setShowResults(true);
      setNoResults(data.length === 0);
    } catch (error) {
      console.error("Error searching appointments:", error);
      setResults([]);
      setShowResults(false);
    } finally {
      setIsSearching(false);
    }
  };

  /**
   * Handle input change with debounce
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setQuery(value);
    setSelectedIndex(-1);

    // Clear existing timer
    if (debounceTimerRef.current !== null) {
      window.clearTimeout(debounceTimerRef.current);
    }

    // Set new debounce timer (300ms)
    debounceTimerRef.current = window.setTimeout(() => {
      searchAppointments(value);
    }, 300);
  };

  /**
   * Handle keyboard navigation
   */
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (!showResults || results.length === 0) return;

    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        setSelectedIndex((prev) =>
          prev < results.length - 1 ? prev + 1 : prev,
        );
        break;
      case "ArrowUp":
        e.preventDefault();
        setSelectedIndex((prev) => (prev > 0 ? prev - 1 : -1));
        break;
      case "Enter":
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < results.length) {
          const selectedResult = results[selectedIndex];
          if (selectedResult) {
            handleSelectAppointment(selectedResult);
          }
        }
        break;
      case "Escape":
        setShowResults(false);
        setSelectedIndex(-1);
        break;
    }
  };

  /**
   * Handle appointment selection
   */
  const handleSelectAppointment = (appointment: ArrivalAppointment) => {
    onSelectAppointment(appointment);
    setQuery("");
    setResults([]);
    setShowResults(false);
    setSelectedIndex(-1);
    setNoResults(false);
  };

  /**
   * Handle no appointment found - navigate to walk-in booking
   */
  const handleCreateWalkin = () => {
    onNoAppointmentFound(query);
  };

  /**
   * Cleanup debounce timer on unmount
   */
  useEffect(() => {
    return () => {
      if (debounceTimerRef.current !== null) {
        window.clearTimeout(debounceTimerRef.current);
      }
    };
  }, []);

  return (
    <div className="relative">
      <label
        htmlFor="arrival-search"
        className="block text-sm font-medium text-neutral-700 mb-2"
      >
        Search for Patient
      </label>
      <div className="relative">
        <input
          ref={inputRef}
          id="arrival-search"
          type="text"
          value={query}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          placeholder="Search by name, email, or phone..."
          className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-primary-500 
                        focus:border-primary-500 transition-colors"
          aria-label="Search for patient with today's appointment"
          aria-autocomplete="list"
          aria-controls="search-results"
          aria-expanded={showResults}
        />
        {isSearching && (
          <div className="absolute right-3 top-3">
            <svg
              className="animate-spin h-5 w-5 text-primary-500"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
              />
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              />
            </svg>
          </div>
        )}
      </div>

      {/* Search Results Dropdown */}
      {showResults && !noResults && results.length > 0 && (
        <ul
          id="search-results"
          role="listbox"
          className="absolute z-10 w-full mt-1 bg-neutral-0 border border-neutral-200 rounded-lg shadow-lg 
                        max-h-80 overflow-auto"
        >
          {results.map((appointment, index) => (
            <li
              key={appointment.appointmentId}
              role="option"
              aria-selected={index === selectedIndex}
              className={`px-4 py-3 cursor-pointer transition-colors border-b border-neutral-100 last:border-b-0
                                ${index === selectedIndex ? "bg-primary-50" : "hover:bg-neutral-50"}`}
              onClick={() => handleSelectAppointment(appointment)}
            >
              <div className="flex justify-between items-start">
                <div>
                  <p className="font-medium text-neutral-900">
                    {appointment.patientName}
                  </p>
                  <p className="text-sm text-neutral-600">
                    DOB: {appointment.dateOfBirth}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-medium text-neutral-900">
                    {new Date(appointment.scheduledDateTime).toLocaleTimeString(
                      "en-US",
                      {
                        hour: "numeric",
                        minute: "2-digit",
                        hour12: true,
                      },
                    )}
                  </p>
                  <p className="text-xs text-neutral-500">
                    {appointment.providerName}
                  </p>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}

      {/* No Results State */}
      {noResults && query.trim().length >= 2 && (
        <div className="absolute z-10 w-full mt-1 bg-neutral-0 border border-neutral-200 rounded-lg shadow-lg p-6 text-center">
          <svg
            className="w-16 h-16 mx-auto text-neutral-300 mb-3"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={1.5}
              d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
          <h3 className="text-lg font-semibold text-neutral-900 mb-2">
            No Appointment Found
          </h3>
          <p className="text-sm text-neutral-600 mb-4">
            No appointment found for today. Would you like to create a walk-in
            booking?
          </p>
          <button
            onClick={handleCreateWalkin}
            className="inline-flex items-center gap-2 px-4 py-2 bg-primary-500 text-neutral-0 font-medium 
                            rounded-lg hover:bg-primary-600 focus:outline-none focus:ring-2 focus:ring-primary-500 
                            focus:ring-offset-2 transition-colors"
          >
            <svg
              className="w-5 h-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 4v16m8-8H4"
              />
            </svg>
            Create Walk-in Booking
          </button>
        </div>
      )}
    </div>
  );
}

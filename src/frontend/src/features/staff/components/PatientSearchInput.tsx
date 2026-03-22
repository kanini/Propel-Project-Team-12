/**
 * PatientSearchInput Component (US_029, AC1 / US_032, AC1)
 * Real-time patient search with 300ms debounce for walk-in booking
 */

import { useState, useEffect, useRef } from "react";
import { searchPatients } from "../../../api/staffApi";
import type { PatientSearchResult } from "../../../types/staff";
import { useDebounce } from "../../../hooks/useDebounce";

interface PatientSearchInputProps {
  onSelectPatient: (patient: PatientSearchResult) => void;
  onCreateNewPatient: () => void;
  placeholder?: string;
}

export function PatientSearchInput({
  onSelectPatient,
  onCreateNewPatient,
  placeholder = "Search patient by name, email, or phone...",
}: PatientSearchInputProps) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<PatientSearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isOpen, setIsOpen] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Debounce search query by 300ms (US_029, AC1)
  const debouncedQuery = useDebounce(query, 300);

  // Perform search when debounced query changes
  useEffect(() => {
    async function performSearch() {
      if (debouncedQuery.trim().length < 2) {
        setResults([]);
        setIsOpen(false);
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const searchResults = await searchPatients(debouncedQuery);
        setResults(searchResults);
        setIsOpen(true);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to search patients",
        );
        setResults([]);
      } finally {
        setIsLoading(false);
      }
    }

    performSearch();
  }, [debouncedQuery]);

  // Handle click outside to close dropdown
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Handle keyboard navigation (US_029, AC1)
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (!isOpen) return;

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
          const selectedPatient = results[selectedIndex];
          if (selectedPatient) {
            handleSelectPatient(selectedPatient);
          }
        }
        break;
      case "Escape":
        e.preventDefault();
        setIsOpen(false);
        setSelectedIndex(-1);
        break;
    }
  };

  const handleSelectPatient = (patient: PatientSearchResult) => {
    onSelectPatient(patient);
    setQuery("");
    setResults([]);
    setIsOpen(false);
    setSelectedIndex(-1);
  };

  const formatDateOfBirth = (dob: string) => {
    const date = new Date(dob);
    return date.toLocaleDateString("en-US", {
      month: "2-digit",
      day: "2-digit",
      year: "numeric",
    });
  };

  const formatLastAppointment = (date?: string) => {
    if (!date) return "No previous appointments";
    const appointmentDate = new Date(date);
    return `Last seen: ${appointmentDate.toLocaleDateString("en-US", { month: "2-digit", day: "2-digit", year: "numeric" })}`;
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          onFocus={() => {
            if (results.length > 0) setIsOpen(true);
          }}
          placeholder={placeholder}
          className="w-full px-4 py-3 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors"
          aria-label="Search for patient"
          aria-autocomplete="list"
          aria-haspopup="listbox"
          aria-expanded={isOpen}
        />
        {isLoading && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            <svg
              className="animate-spin h-5 w-5 text-primary-500"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
              ></circle>
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              ></path>
            </svg>
          </div>
        )}
      </div>

      {/* Error message */}
      {error && (
        <div className="mt-2 text-sm text-error-600" role="alert">
          {error}
        </div>
      )}

      {/* Dropdown results (US_029, AC1) */}
      {isOpen && !isLoading && (
        <div className="absolute z-50 w-full mt-2 bg-white border border-neutral-200 rounded-lg shadow-lg max-h-96 overflow-y-auto">
          {results.length === 0 && debouncedQuery.length >= 2 ? (
            <div className="p-4">
              <p className="text-neutral-600 mb-3">
                No patients found matching "{debouncedQuery}"
              </p>
              <button
                type="button"
                onClick={onCreateNewPatient}
                className="w-full px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors font-medium"
              >
                Create New Patient
              </button>
            </div>
          ) : (
            <ul role="listbox" className="py-2">
              {results.map((patient, index) => (
                <li
                  key={patient.id}
                  role="option"
                  aria-selected={index === selectedIndex}
                  onClick={() => handleSelectPatient(patient)}
                  className={`px-4 py-3 cursor-pointer transition-colors ${
                    index === selectedIndex
                      ? "bg-primary-50"
                      : "hover:bg-neutral-50"
                  }`}
                >
                  <div className="flex flex-col">
                    <span className="font-semibold text-neutral-900">
                      {patient.fullName}
                    </span>
                    <div className="flex flex-wrap gap-3 mt-1 text-sm text-neutral-600">
                      <span>DOB: {formatDateOfBirth(patient.dateOfBirth)}</span>
                      {patient.email && <span>Email: {patient.email}</span>}
                      <span>Phone: {patient.phone}</span>
                    </div>
                    <span className="text-xs text-neutral-500 mt-1">
                      {formatLastAppointment(
                        patient.lastAppointmentDate || undefined,
                      )}
                    </span>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}

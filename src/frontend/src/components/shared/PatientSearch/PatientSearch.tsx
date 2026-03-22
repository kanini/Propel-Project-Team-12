/**
 * PatientSearch Component for US_032
 * Reusable patient search with debounce, dropdown results, and keyboard navigation
 */

import { useState, useRef, useEffect } from "react";
import { usePatientSearch } from "../../../hooks/usePatientSearch";
import { PatientSearchResult as PatientSearchResultRow } from "./PatientSearchResult";
import type {
  PatientSearchProps,
  PatientSearchResult as PatientSearchResultType,
} from "./types";

/**
 * Input sanitization regex - allows letters (including accented), numbers, spaces, hyphens, apostrophes, @ and .
 */
const ALLOWED_CHARACTERS = /^[a-zA-Z0-9\s\-'À-ÿ@.]*$/;

/**
 * Reusable patient search component with real-time filtering
 */
export function PatientSearch({
  onSelectPatient,
  showCreateButton = false,
  placeholder = "Search by name, email, or phone...",
  onCreatePatient,
  clearOnSelect = true,
  className = "",
}: PatientSearchProps) {
  const [query, setQuery] = useState("");
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [showDropdown, setShowDropdown] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const resultCountRef = useRef<HTMLDivElement>(null);

  const { results, isLoading, error, clearResults } = usePatientSearch(query);

  /**
   * Handle input change with sanitization
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;

    // Sanitize input - allow only safe characters
    if (value === "" || ALLOWED_CHARACTERS.test(value)) {
      setQuery(value);
      setSelectedIndex(-1);
      setShowDropdown(true);
    }
  };

  /**
   * Handle patient selection
   */
  const handleSelectPatient = (patient: PatientSearchResultType) => {
    onSelectPatient(patient);

    if (clearOnSelect) {
      setQuery("");
      clearResults();
    }

    setShowDropdown(false);
    setSelectedIndex(-1);
  };

  /**
   * Handle keyboard navigation
   */
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (!showDropdown || results.length === 0) return;

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
            handleSelectPatient(selectedResult);
          }
        }
        break;

      case "Escape":
        setShowDropdown(false);
        setSelectedIndex(-1);
        break;
    }
  };

  /**
   * Handle click outside to close dropdown
   */
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node) &&
        inputRef.current &&
        !inputRef.current.contains(event.target as Node)
      ) {
        setShowDropdown(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  /**
   * Handle input focus
   */
  const handleFocus = () => {
    if (query.trim().length >= 2 && results.length > 0) {
      setShowDropdown(true);
    }
  };

  /**
   * Handle create patient button click
   */
  const handleCreatePatient = () => {
    if (onCreatePatient) {
      onCreatePatient(query);
    }
    setShowDropdown(false);
  };

  // Announce result count for screen readers
  const resultCountAnnouncement =
    results.length > 0
      ? `${results.length} patient${results.length === 1 ? "" : "s"} found`
      : query.trim().length >= 2 && !isLoading
        ? "No patients found"
        : "";

  return (
    <div className={`relative ${className}`}>
      {/* Search Input */}
      <div className="relative">
        <label
          htmlFor="patient-search"
          className="block text-sm font-medium text-neutral-700 mb-2"
        >
          Search for Patient
        </label>
        <input
          ref={inputRef}
          id="patient-search"
          type="text"
          value={query}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          onFocus={handleFocus}
          placeholder={placeholder}
          className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-primary-500 
                        focus:border-primary-500 transition-colors"
          role="combobox"
          aria-label="Search for patient"
          aria-autocomplete="list"
          aria-controls="patient-search-results"
          aria-expanded={
            showDropdown &&
            (results.length > 0 || (query.trim().length >= 2 && !isLoading))
          }
          aria-activedescendant={
            selectedIndex >= 0 ? `result-${selectedIndex}` : undefined
          }
        />

        {/* Loading Spinner */}
        {isLoading && (
          <div className="absolute right-3 top-10">
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

      {/* ARIA Live Region for Result Count */}
      <div
        ref={resultCountRef}
        className="sr-only"
        role="status"
        aria-live="polite"
        aria-atomic="true"
      >
        {resultCountAnnouncement}
      </div>

      {/* Dropdown Results */}
      {showDropdown && query.trim().length >= 2 && !isLoading && (
        <div
          ref={dropdownRef}
          id="patient-search-results"
          role="listbox"
          className="absolute z-50 w-full mt-1 bg-neutral-0 border border-neutral-200 rounded-lg shadow-lg 
                        max-h-96 overflow-auto"
        >
          {/* Error State */}
          {error && (
            <div className="px-4 py-3 text-sm text-error">
              <p>{error}</p>
            </div>
          )}

          {/* Results List */}
          {!error && results.length > 0 && (
            <div role="group" aria-label="Search results">
              {results.map((patient, index) => (
                <div
                  key={patient.id}
                  id={`result-${index}`}
                  role="option"
                  aria-selected={index === selectedIndex}
                >
                  <PatientSearchResultRow
                    patient={patient}
                    isSelected={index === selectedIndex}
                    onSelect={() => handleSelectPatient(patient)}
                  />
                </div>
              ))}
            </div>
          )}

          {/* Empty State */}
          {!error && results.length === 0 && (
            <div className="px-6 py-8 text-center">
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
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
              <p className="text-sm font-medium text-neutral-900 mb-1">
                No patients found
              </p>
              <p className="text-xs text-neutral-600 mb-4">
                No patients matching "{query}"
              </p>

              {showCreateButton && onCreatePatient && (
                <button
                  onClick={handleCreatePatient}
                  className="inline-flex items-center gap-2 px-4 py-2 bg-primary-500 text-neutral-0 
                                        font-medium rounded-lg hover:bg-primary-600 focus:outline-none focus:ring-2 
                                        focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                >
                  <svg
                    className="w-4 h-4"
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
                  Create New Patient
                </button>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

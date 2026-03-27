/**
 * ModifyCodeModal Component (EP-008-US-052, AC3)
 * Modal for modifying AI-suggested codes with alternative codes
 * Includes search functionality and rationale input
 */

import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { modifyCode } from '../../store/slices/medicalCodeVerificationSlice';
import type {
  MedicalCodeSuggestion,
  CodeSearchResult,
} from '../../types/medicalCode.types';
import type { AppDispatch } from '../../store';
import { toast } from '../../utils/toast';

interface ModifyCodeModalProps {
  code: MedicalCodeSuggestion;
  isOpen: boolean;
  onClose: () => void;
}

export const ModifyCodeModal: React.FC<ModifyCodeModalProps> = ({
  code,
  isOpen,
  onClose,
}) => {
  const dispatch = useDispatch<AppDispatch>();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCode, setSelectedCode] = useState<{
    code: string;
    description: string;
  } | null>(null);
  const [rationale, setRationale] = useState('');
  const [searchResults, setSearchResults] = useState<CodeSearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSearch = async () => {
    if (!searchQuery.trim()) {
      toast.warning('Please enter a search query');
      return;
    }

    setIsSearching(true);
    try {
      // Integration with task_002_be_code_search_service (EP-008-US-052 dependent task)
      const response = await fetch(
        `/api/knowledge/search?query=${encodeURIComponent(searchQuery)}&codeSystem=${code.codeSystem}&topK=10`
      );

      if (!response.ok) {
        throw new Error('Search failed');
      }

      const data = await response.json();
      setSearchResults(data.results || []);

      if (!data.results || data.results.length === 0) {
        toast.info('No results found. Try a different search term.');
      }
    } catch (error) {
      toast.error('Failed to search codes');
      setSearchResults([]);
    } finally {
      setIsSearching(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const handleSave = async () => {
    if (!selectedCode || !rationale.trim()) {
      toast.error('Please select a code and provide rationale');
      return;
    }

    setIsSubmitting(true);
    try {
      await dispatch(
        modifyCode({
          codeId: code.id,
          newCode: selectedCode.code,
          newDescription: selectedCode.description,
          rationale: rationale.trim(),
        })
      ).unwrap();

      toast.success('Code modified successfully', { autoClose: 200 }); // UXR-501: 200ms feedback
      onClose();
      // Reset state
      setSearchQuery('');
      setSelectedCode(null);
      setRationale('');
      setSearchResults([]);
    } catch (error) {
      toast.error('Failed to modify code');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      role="dialog"
      aria-modal="true"
      aria-labelledby="modify-modal-title"
    >
      <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl p-6 max-h-[90vh] overflow-y-auto">
        <h3 id="modify-modal-title" className="text-xl font-semibold mb-4">
          Modify Code
        </h3>

        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Current Code
          </label>
          <div className="p-3 bg-gray-50 rounded">
            <span className="font-mono font-semibold">{code.code}</span>
            <span className="text-gray-600 ml-2">— {code.description}</span>
          </div>
        </div>

        <div className="mb-4">
          <label
            htmlFor="code-search"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Search Alternative Codes
          </label>
          <div className="flex gap-2">
            <input
              id="code-search"
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder={`Search ${code.codeSystem} codes...`}
              className="flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              aria-label={`Search ${code.codeSystem} codes`}
            />
            <button
              onClick={handleSearch}
              disabled={isSearching}
              className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 transition-colors"
            >
              {isSearching ? 'Searching...' : 'Search'}
            </button>
          </div>
        </div>

        {searchResults.length > 0 && (
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Search Results
            </label>
            <div className="max-h-64 overflow-y-auto border rounded-lg">
              {searchResults.map((result, index) => (
                <div
                  key={index}
                  onClick={() =>
                    setSelectedCode({
                      code: result.code,
                      description: result.description,
                    })
                  }
                  className={`p-3 cursor-pointer hover:bg-gray-50 border-b last:border-b-0 transition-colors ${
                    selectedCode?.code === result.code ? 'bg-blue-50' : ''
                  }`}
                  role="button"
                  tabIndex={0}
                  onKeyPress={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      setSelectedCode({
                        code: result.code,
                        description: result.description,
                      });
                    }
                  }}
                  aria-label={`Select ${result.code} - ${result.description}`}
                >
                  <span className="font-mono font-semibold">
                    {result.code}
                  </span>
                  <span className="text-gray-600 ml-2">
                    — {result.description}
                  </span>
                  <span className="text-xs text-gray-500 ml-2">
                    (Similarity: {result.similarityScore}%)
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}

        <div className="mb-6">
          <label
            htmlFor="rationale"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Modification Rationale <span className="text-red-600">*</span>
          </label>
          <textarea
            id="rationale"
            value={rationale}
            onChange={(e) => setRationale(e.target.value)}
            rows={3}
            placeholder="Explain why you are modifying this code..."
            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            required
            aria-required="true"
          />
        </div>

        <div className="flex justify-end gap-3">
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={!selectedCode || !rationale.trim() || isSubmitting}
            className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSubmitting ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </div>
    </div>
  );
};

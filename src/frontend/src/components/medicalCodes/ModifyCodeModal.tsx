/**
 * Modify Code Modal Component
 * Allows staff to search for and select alternative medical codes (AC3)
 * Integrates with code search service from US_050
 */

import React, { useState } from 'react';

 import { useDispatch } from 'react-redux';
import { toast } from 'react-toastify';
import { modifyCode } from '../../store/slices/medicalCodeVerificationSlice';
import { medicalCodesApi } from '../../api/medicalCodesApi';
import type {
  MedicalCodeSuggestion,
  CodeSearchResult,
} from '../../types/medicalCode.types';
import type { AppDispatch } from '../../store/index';

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

  const handleSearch = async () => {
    if (!searchQuery.trim()) {
      toast.error('Please enter a search query');
      return;
    }

    setIsSearching(true);
    try {
      const results = await medicalCodesApi.searchCodes(
        searchQuery,
        code.codeSystem,
        10
      );
      setSearchResults(results);
    } catch (error: any) {
      toast.error(
        error.response?.data?.message || 'Failed to search codes'
      );
    } finally {
      setIsSearching(false);
    }
  };

  const handleSave = async () => {
    if (!selectedCode || !rationale) {
      toast.error('Please select a code and provide rationale');
      return;
    }

    try {
      await dispatch(
        modifyCode({
          codeId: code.id,
          newCode: selectedCode.code,
          newDescription: selectedCode.description,
          rationale,
        })
      ).unwrap();

      toast.success('Code modified successfully', { autoClose: 200 }); // UXR-501: 200ms feedback
      onClose();
    } catch (error: any) {
      toast.error(error || 'Failed to modify code');
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-lg shadow-xl w-full max-w-2xl p-6"
        onClick={(e) => e.stopPropagation()}
      >
        <h3 className="text-xl font-semibold mb-4">Modify Code</h3>

        {/* Current Code Display */}
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Current Code
          </label>
          <div className="p-3 bg-gray-50 rounded">
            <span className="font-mono font-semibold">{code.code}</span>
            <span className="text-gray-600 ml-2">— {code.description}</span>
          </div>
        </div>

        {/* Code Search */}
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Search Alternative Codes
          </label>
          <div className="flex gap-2">
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder={`Search ${code.codeSystem} codes...`}
              className="flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
            <button
              onClick={handleSearch}
              disabled={isSearching}
              className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSearching ? 'Searching...' : 'Search'}
            </button>
          </div>
        </div>

        {/* Search Results */}
        {searchResults.length > 0 && (
          <div className="mb-4 max-h-64 overflow-y-auto border rounded-lg">
            {searchResults.map((result, index) => (
              <div
                key={index}
                onClick={() =>
                  setSelectedCode({
                    code: result.code,
                    description: result.description,
                  })
                }
                className={`p-3 cursor-pointer hover:bg-gray-50 border-b last:border-b-0 ${
                  selectedCode?.code === result.code ? 'bg-blue-50' : ''
                }`}
              >
                <span className="font-mono font-semibold">{result.code}</span>
                <span className="text-gray-600 ml-2">
                  — {result.description}
                </span>
                <span className="text-xs text-gray-500 ml-2">
                  (Similarity: {Math.round(result.similarityScore)}%)
                </span>
              </div>
            ))}
          </div>
        )}

        {/* Modification Rationale */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Modification Rationale <span className="text-red-500">*</span>
          </label>
          <textarea
            value={rationale}
            onChange={(e) => setRationale(e.target.value)}
            rows={3}
            placeholder="Explain why you are modifying this code..."
            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
            required
          />
        </div>

        {/* Action Buttons */}
        <div className="flex justify-end gap-3">
          <button
            onClick={onClose}
            className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={!selectedCode || !rationale}
            className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Save Changes
          </button>
        </div>
      </div>
    </div>
  );
};

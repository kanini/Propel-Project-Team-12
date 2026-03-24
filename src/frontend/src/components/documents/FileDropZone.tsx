/**
 * File drop zone component for drag-and-drop or click-to-upload (US_042, SCR-014).
 * Matches wireframe SCR-014 design.
 */

import { useState, useRef } from 'react';

interface FileDropZoneProps {
  onFileSelected: (file: File) => void;
  disabled?: boolean;
}

export function FileDropZone({ onFileSelected, disabled }: FileDropZoneProps) {
  const [isDragOver, setIsDragOver] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
  const ALLOWED_MIME_TYPE = 'application/pdf';

  const validateFile = (file: File): string | null => {
    if (file.type !== ALLOWED_MIME_TYPE) {
      return 'Only PDF files are supported';
    }
    if (file.size > MAX_FILE_SIZE) {
      return 'File size must not exceed 10 MB';
    }
    return null;
  };

  const handleFile = (file: File) => {
    const error = validateFile(file);
    if (error) {
      setValidationError(error);
      return;
    }
    setValidationError(null);
    onFileSelected(file);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);

    if (disabled) return;

    const files = Array.from(e.dataTransfer.files);
    if (files.length > 0 && files[0]) {
      handleFile(files[0]);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!disabled) {
      setIsDragOver(true);
    }
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);
  };

  const handleClick = () => {
    if (!disabled) {
      fileInputRef.current?.click();
    }
  };

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length > 0 && files[0]) {
      handleFile(files[0]);
    }
  };

  return (
    <div className="w-full">
      <div
        onClick={handleClick}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        className={`
          relative border-2 border-dashed rounded-lg py-12 px-6 text-center cursor-pointer
          transition-all duration-200
          ${isDragOver ? 'border-primary-500 bg-primary-50' : 'border-neutral-300 bg-white'}
          ${disabled ? 'opacity-50 cursor-not-allowed' : 'hover:border-primary-500 hover:bg-primary-50'}
        `}
        role="button"
        tabIndex={disabled ? -1 : 0}
        aria-label="Drop files here or click to browse"
      >
        <div className="flex flex-col items-center space-y-3">
          {/* Document icon matching wireframe */}
          <svg className="w-10 h-10 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          
          <div>
            <h3 className="text-lg font-semibold text-neutral-700 mb-1">
              Drag and drop files here
            </h3>
            <p className="text-sm text-neutral-500">
              or{' '}
              <span className="text-primary-500 font-medium cursor-pointer hover:text-primary-600">
                browse your computer
              </span>
            </p>
          </div>
          
          <p className="text-xs text-neutral-400 mt-2">
            PDF files only · Max 10 MB
          </p>
        </div>
      </div>

      {validationError && (
        <div className="mt-3 flex items-start gap-2 p-3 bg-red-50 border border-red-200 rounded-md" role="alert">
          <svg className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
          <p className="text-sm text-red-600">{validationError}</p>
        </div>
      )}

      <input
        ref={fileInputRef}
        type="file"
        accept=".pdf,application/pdf"
        onChange={handleFileInput}
        className="hidden"
        aria-label="File input"
      />
    </div>
  );
}

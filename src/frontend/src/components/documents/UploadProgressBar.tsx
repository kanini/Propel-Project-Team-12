/**
 * Upload progress bar component with real-time updates (US_042, AC2).
 */

interface UploadProgressBarProps {
  progress: number; // 0-100
  status: string;
  fileName: string;
  chunksReceived?: number;
  totalChunks?: number;
}

export function UploadProgressBar({ progress, status, fileName, chunksReceived, totalChunks }: UploadProgressBarProps) {
  const getStatusColor = () => {
    switch (status) {
      case 'complete':
      case 'chunks_uploaded':
        return 'bg-green-500';
      case 'submitting':
        return 'bg-blue-500';
      case 'error':
        return 'bg-red-500';
      case 'paused':
        return 'bg-yellow-500';
      default:
        return 'bg-blue-500';
    }
  };

  const getStatusText = () => {
    switch (status) {
      case 'validating':
        return 'Validating...';
      case 'uploading':
        return `Uploading ${progress}%`;
      case 'chunks_uploaded':
        return 'Uploaded — Ready to submit';
      case 'submitting':
        return 'Submitting for processing...';
      case 'complete':
        return 'Submitted — Processing queued';
      case 'error':
        return 'Upload Failed';
      case 'paused':
        return 'Upload Paused — Retrying...';
      default:
        return 'Preparing...';
    }
  };

  return (
    <div className="w-full space-y-2" role="status" aria-live="polite">
      <div className="flex items-center justify-between text-sm">
        <span className="font-medium text-gray-700 truncate max-w-xs">{fileName}</span>
        <span className="text-gray-500 text-xs">
          {chunksReceived !== undefined && totalChunks ? `${chunksReceived}/${totalChunks} chunks` : getStatusText()}
        </span>
      </div>
      
      <div className="w-full bg-gray-200 rounded-full h-2.5 overflow-hidden">
        <div
          className={`h-2.5 rounded-full transition-all duration-300 ${getStatusColor()}`}
          style={{ width: `${Math.min(progress, 100)}%` }}
          role="progressbar"
          aria-valuenow={progress}
          aria-valuemin={0}
          aria-valuemax={100}
        />
      </div>

      <div className="flex items-center justify-between text-xs text-gray-500">
        <span>{getStatusText()}</span>
        <span>{progress}%</span>
      </div>
    </div>
  );
}

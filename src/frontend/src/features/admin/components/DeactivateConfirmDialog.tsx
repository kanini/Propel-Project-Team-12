import { type User } from '../../../store/usersSlice';

interface DeactivateConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  user: User | null;
}

/**
 * Confirmation dialog for user deactivation (US_021, Edge case).
 * Warns about account deactivation consequences.
 */
export const DeactivateConfirmDialog = ({
  isOpen,
  onClose,
  onConfirm,
  user,
}: DeactivateConfirmDialogProps) => {
  if (!isOpen || !user) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        {/* Backdrop */}
        <div
          className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
          onClick={onClose}
        ></div>

        {/* Dialog */}
        <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full">
          {/* Icon */}
          <div className="px-6 pt-6">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
              <svg
                className="h-6 w-6 text-red-600"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth="1.5"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"
                />
              </svg>
            </div>
          </div>

          {/* Content */}
          <div className="px-6 py-4 text-center">
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              Deactivate User Account
            </h3>
            <p className="text-sm text-gray-500 mb-4">
              Are you sure you want to deactivate <strong>{user.name}</strong>'s account?
            </p>
            <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3 text-left">
              <p className="text-sm text-yellow-800">
                <strong>Warning:</strong> This action will:
              </p>
              <ul className="mt-2 text-sm text-yellow-700 list-disc list-inside space-y-1">
                <li>Set the account status to Inactive</li>
                <li>Terminate all active sessions immediately</li>
                <li>Prevent future logins</li>
                <li>Create an audit log entry</li>
              </ul>
            </div>
          </div>

          {/* Actions */}
          <div className="px-6 py-4 bg-gray-50 flex justify-end space-x-3 rounded-b-lg">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={() => {
                onConfirm();
                onClose();
              }}
              className="px-4 py-2 text-sm font-medium text-white bg-red-600 border border-transparent rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
            >
              Deactivate Account
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

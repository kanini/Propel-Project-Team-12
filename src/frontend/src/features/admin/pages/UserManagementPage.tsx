import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useAuth } from '../../../hooks/useAuth';
import type { AppDispatch } from '../../../store';
import {
  fetchUsers,
  createUser,
  updateUser,
  deactivateUser,
  setSearchTerm,
  selectUsers,
  selectIsLoading,
  selectError,
  selectSearchTerm,
  type User,
} from '../../../store/usersSlice';
import { UserTable } from '../components/UserTable';
import { UserFormModal } from '../components/UserFormModal';
import { DeactivateConfirmDialog } from '../components/DeactivateConfirmDialog';

/**
 * User management page for Admin (US_021).
 * Displays user list with search, create, edit, and deactivate functionality.
 */
export const UserManagementPage = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { userId: currentUserId } = useAuth();
  
  const users = useSelector(selectUsers);
  const isLoading = useSelector(selectIsLoading);
  const error = useSelector(selectError);
  const searchTerm = useSelector(selectSearchTerm);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeactivateDialogOpen, setIsDeactivateDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [formMode, setFormMode] = useState<'create' | 'edit'>('create');

  useEffect(() => {
    dispatch(fetchUsers({ searchTerm }));
  }, [dispatch, searchTerm]);

  const handleSearch = (value: string) => {
    dispatch(setSearchTerm(value));
  };

  const handleCreateUser = () => {
    setFormMode('create');
    setSelectedUser(null);
    setIsFormOpen(true);
  };

  const handleEditUser = (user: User) => {
    setFormMode('edit');
    setSelectedUser(user);
    setIsFormOpen(true);
  };

  const handleDeactivateUser = (user: User) => {
    setSelectedUser(user);
    setIsDeactivateDialogOpen(true);
  };

  const handleFormSubmit = async (userData: any) => {
    try {
      if (formMode === 'create') {
        await dispatch(createUser(userData)).unwrap();
      } else if (selectedUser) {
        await dispatch(
          updateUser({ userId: selectedUser.userId, userData })
        ).unwrap();
      }
      setIsFormOpen(false);
      setSelectedUser(null);
      // Refresh user list
      dispatch(fetchUsers({ searchTerm }));
    } catch (error) {
      console.error('Failed to save user:', error);
    }
  };

  const handleConfirmDeactivate = async () => {
    if (selectedUser) {
      try {
        await dispatch(deactivateUser(selectedUser.userId)).unwrap();
        setSelectedUser(null);
        // Refresh user list
        dispatch(fetchUsers({ searchTerm }));
      } catch (error) {
        console.error('Failed to deactivate user:', error);
      }
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">User Management</h1>
        <p className="mt-1 text-sm text-gray-500">
          Manage staff and admin accounts
        </p>
      </div>

      {/* Error Alert */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <div className="flex">
            <svg
              className="h-5 w-5 text-red-400"
              viewBox="0 0 20 20"
              fill="currentColor"
            >
              <path
                fillRule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                clipRule="evenodd"
              />
            </svg>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">Error</h3>
              <p className="mt-1 text-sm text-red-700">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* Search and Create Button */}
      <div className="flex flex-col sm:flex-row gap-4 justify-between items-start sm:items-center">
        {/* Search Bar (UXR-004: Inline search) */}
        <div className="relative flex-1 max-w-md w-full">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg
              className="h-5 w-5 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              />
            </svg>
          </div>
          <input
            type="text"
            placeholder="Search by name or email..."
            value={searchTerm}
            onChange={(e) => handleSearch(e.target.value)}
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
          />
        </div>

        {/* Create User Button */}
        <button
          onClick={handleCreateUser}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <svg
            className="-ml-1 mr-2 h-5 w-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 4v16m8-8H4"
            />
          </svg>
          Create User
        </button>
      </div>

      {/* User Table */}
      <UserTable
        users={users}
        isLoading={isLoading}
        onEdit={handleEditUser}
        onDeactivate={handleDeactivateUser}
        currentUserId={currentUserId}
      />

      {/* User Form Modal */}
      <UserFormModal
        isOpen={isFormOpen}
        onClose={() => {
          setIsFormOpen(false);
          setSelectedUser(null);
        }}
        onSubmit={handleFormSubmit}
        user={selectedUser}
        title={formMode === 'create' ? 'Create New User' : 'Edit User'}
      />

      {/* Deactivate Confirmation Dialog */}
      <DeactivateConfirmDialog
        isOpen={isDeactivateDialogOpen}
        onClose={() => {
          setIsDeactivateDialogOpen(false);
          setSelectedUser(null);
        }}
        onConfirm={handleConfirmDeactivate}
        user={selectedUser}
      />
    </div>
  );
};

export default UserManagementPage;

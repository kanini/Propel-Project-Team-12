import React, { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import {
  fetchUsers,
  fetchActiveAdminCount,
  createUser,
  updateUser,
  deactivateUser,
  clearError
} from '../../store/slices/userManagementSlice';
import { UserRole, UserStatus } from '../../types/user.types';
import type { User, CreateUserRequest, UpdateUserRequest } from '../../types/user.types';
import { getUserId } from '../../utils/tokenStorage';

/**
 * User Management Page (US_021, SCR-021).
 * Admin-only interface for creating, updating, and deactivating Staff/Admin users.
 */
const UserManagementPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { users, loading, error, activeAdminCount } = useAppSelector(state => state.userManagement);
  const currentUserId = getUserId();

  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('createdAt');
  const [ascending, setAscending] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeactivateDialog, setShowDeactivateDialog] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);

  useEffect(() => {
    dispatch(fetchUsers({ search, sortBy, ascending }));
    dispatch(fetchActiveAdminCount());
  }, [dispatch, search, sortBy, ascending]);

  const handleCreateUser = async (request: CreateUserRequest) => {
    const result = await dispatch(createUser(request));
    if (!result.payload) {
      alert(error || 'Failed to create user');
      return;
    }
    setShowCreateModal(false);
    dispatch(fetchUsers({ search, sortBy, ascending }));
  };

  const handleUpdateUser = async (userId: string, request: UpdateUserRequest) => {
    const result = await dispatch(updateUser({ id: userId, request }));
    if (!result.payload) {
      alert(error || 'Failed to update user');
      return;
    }
    setShowEditModal(false);
    setSelectedUser(null);
  };

  const handleDeactivateUser = async (userId: string) => {
    if (userId === currentUserId) {
      alert('Cannot deactivate your own account');
      return;
    }

    const user = users.find(u => u.userId === userId);
    if (user?.role === UserRole.Admin && activeAdminCount <= 1) {
      alert('Cannot deactivate the last active Admin account');
      return;
    }

    const result = await dispatch(deactivateUser(userId));
    if (!result.payload) {
      alert(error || 'Failed to deactivate user');
      return;
    }
    setShowDeactivateDialog(false);
    setSelectedUser(null);
  };

  const getRoleLabel = (role: UserRole): string => {
    switch (role) {
      case UserRole.Patient: return 'Patient';
      case UserRole.Staff: return 'Staff';
      case UserRole.Admin: return 'Admin';
      default: return 'Unknown';
    }
  };

  const getStatusLabel = (status: UserStatus): string => {
    switch (status) {
      case UserStatus.Active: return 'Active';
      case UserStatus.Suspended: return 'Suspended';
      case UserStatus.Inactive: return 'Inactive';
      default: return 'Unknown';
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">User Management</h1>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md transition-colors"
        >
          Create User
        </button>
      </div>

      {/* Search and Sort Controls */}
      <div className="mb-6 flex gap-4">
        <input
          type="text"
          placeholder="Search by name or email..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
        <select
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"
        >
          <option value="name">Name</option>
          <option value="email">Email</option>
          <option value="role">Role</option>
          <option value="status">Status</option>
          <option value="createdAt">Created Date</option>
        </select>
        <button
          onClick={() => setAscending(!ascending)}
          className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
        >
          {ascending ? '↑ Asc' : '↓ Desc'}
        </button>
      </div>

      {/* Error Display */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-md mb-4">
          {error}
          <button onClick={() => dispatch(clearError())} className="float-right font-bold">×</button>
        </div>
      )}

      {/* User Table */}
      {loading ? (
        <div className="text-center py-12">Loading users...</div>
      ) : users.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <p className="text-gray-600 text-lg mb-4">No users found</p>
          <button
            onClick={() => setShowCreateModal(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-md"
          >
            Create First User
          </button>
        </div>
      ) : (
        <div className="overflow-x-auto bg-white shadow-md rounded-lg">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {users.map((user) => (
                <tr key={user.userId} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{user.name}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{user.email}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      user.role === UserRole.Admin ? 'bg-purple-100 text-purple-800' :
                      user.role === UserRole.Staff ? 'bg-blue-100 text-blue-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {getRoleLabel(user.role)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      user.status === UserStatus.Active ? 'bg-green-100 text-green-800' :
                      user.status === UserStatus.Suspended ? 'bg-yellow-100 text-yellow-800' :
                      'bg-red-100 text-red-800'
                    }`}>
                      {getStatusLabel(user.status)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                    {new Date(user.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                    <button
                      onClick={() => { setSelectedUser(user); setShowEditModal(true); }}
                      className="text-blue-600 hover:text-blue-900"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => { setSelectedUser(user); setShowDeactivateDialog(true); }}
                      className="text-red-600 hover:text-red-900"
                      disabled={user.userId === currentUserId}
                    >
                      Deactivate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create User Modal */}
      {showCreateModal && (
        <UserFormModal
          mode="create"
          onClose={() => setShowCreateModal(false)}
          onSubmit={(request) => handleCreateUser(request as CreateUserRequest)}
        />
      )}

      {/* Edit User Modal */}
      {showEditModal && selectedUser && (
        <UserFormModal
          mode="edit"
          user={selectedUser}
          onClose={() => { setShowEditModal(false); setSelectedUser(null); }}
          onSubmit={(request) => handleUpdateUser(selectedUser.userId, request as UpdateUserRequest)}
        />
      )}

      {/* Deactivate Confirmation Dialog */}
      {showDeactivateDialog && selectedUser && (
        <DeactivateUserDialog
          user={selectedUser}
          onConfirm={() => handleDeactivateUser(selectedUser.userId)}
          onCancel={() => { setShowDeactivateDialog(false); setSelectedUser(null); }}
        />
      )}
    </div>
  );
};

/**
 * User Form Modal for creating and editing users.
 */
interface UserFormModalProps {
  mode: 'create' | 'edit';
  user?: User;
  onClose: () => void;
  onSubmit: (request: CreateUserRequest | UpdateUserRequest) => void;
}

const UserFormModal: React.FC<UserFormModalProps> = ({ mode, user, onClose, onSubmit }) => {
  const [name, setName] = useState(user?.name || '');
  const [email, setEmail] = useState(user?.email || '');
  const [role, setRole] = useState<UserRole>(user?.role || UserRole.Staff);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (mode === 'create') {
      onSubmit({ name, email, role } as CreateUserRequest);
    } else {
      onSubmit({ name, role } as UpdateUserRequest);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-8 max-w-md w-full">
        <h2 className="text-2xl font-bold mb-4">{mode === 'create' ? 'Create User' : 'Edit User'}</h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">Name</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              minLength={2}
              maxLength={100}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"
            />
          </div>

         {mode === 'create' && (
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"
              />
            </div>
          )}

          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">Role</label>
            <select
              value={role}
              onChange={(e) => setRole(Number(e.target.value) as UserRole)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"
            >
              <option value={UserRole.Staff}>Staff</option>
              <option value={UserRole.Admin}>Admin</option>
            </select>
          </div>

          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md"
            >
              {mode === 'create' ? 'Create' : 'Update'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

/**
 * Deactivate User Confirmation Dialog.
 */
interface DeactivateUserDialogProps {
  user: User;
  onConfirm: () => void;
  onCancel: () => void;
}

const DeactivateUserDialog: React.FC<DeactivateUserDialogProps> = ({ user, onConfirm, onCancel }) => {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-8 max-w-md w-full">
        <h2 className="text-2xl font-bold text-red-600 mb-4">Deactivate User</h2>
        <p className="text-gray-700 mb-6">
          Are you sure you want to deactivate <strong>{user.name}</strong> ({user.email})?
          This will terminate all active sessions and block future logins.
        </p>
        <div className="flex justify-end space-x-3">
          <button
            onClick={onCancel}
            className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md"
          >
            Deactivate
          </button>
        </div>
      </div>
    </div>
  );
};

export default UserManagementPage;

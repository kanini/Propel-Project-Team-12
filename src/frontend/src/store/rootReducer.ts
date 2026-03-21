import { combineReducers } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import usersReducer from './usersSlice';

// Import your slice reducers here
// Example: import authReducer from './slices/authSlice';

const rootReducer = combineReducers({
  auth: authReducer,
  users: usersReducer,
  // Add your slice reducers here
  // Example: auth: authReducer,
});

export default rootReducer;

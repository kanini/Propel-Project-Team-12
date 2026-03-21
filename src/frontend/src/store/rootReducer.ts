import { combineReducers } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';
import userManagementReducer from './slices/userManagementSlice';

// Import your slice reducers here
// Example: import authReducer from './slices/authSlice';

const rootReducer = combineReducers({
  // Add your slice reducers here
  auth: authReducer,
  userManagement: userManagementReducer,
});

export default rootReducer;

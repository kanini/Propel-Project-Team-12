import { combineReducers } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import usersReducer from './usersSlice';
import providerReducer from './slices/providerSlice';
import appointmentReducer from './slices/appointmentSlice';
import waitlistReducer from './slices/waitlistSlice';

// Import your slice reducers here
// Example: import authReducer from './slices/authSlice';

const rootReducer = combineReducers({
  auth: authReducer,
  users: usersReducer,
  providers: providerReducer,
  appointments: appointmentReducer,
  waitlist: waitlistReducer,
  // Add your slice reducers here
  // Example: auth: authReducer,
});

export default rootReducer;

import { combineReducers } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import usersReducer from './usersSlice';
import providerReducer from './slices/providerSlice';
import appointmentReducer from './slices/appointmentSlice';
import waitlistReducer from './slices/waitlistSlice';
import documentsReducer from './documentsSlice';
import settingsReducer from './slices/settingsSlice';
import intakeAppointmentReducer from './slices/intakeAppointmentSlice';
import intakeReducer from './slices/intakeSlice';

const rootReducer = combineReducers({
  auth: authReducer,
  users: usersReducer,
  providers: providerReducer,
  appointments: appointmentReducer,
  waitlist: waitlistReducer,
  documents: documentsReducer,
  settings: settingsReducer, // US_037 - Admin system settings
  intakeAppointments: intakeAppointmentReducer,
  intake: intakeReducer,
});

export default rootReducer;

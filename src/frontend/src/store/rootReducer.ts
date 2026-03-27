import { combineReducers } from "@reduxjs/toolkit";
import authReducer from "../features/auth/authSlice";
import usersReducer from "./usersSlice";
import providerReducer from "./slices/providerSlice";
import appointmentReducer from "./slices/appointmentSlice";
import waitlistReducer from "./slices/waitlistSlice";
import documentsReducer from "./documentsSlice";
import intakeAppointmentReducer from "./slices/intakeAppointmentSlice";
import intakeReducer from "./slices/intakeSlice";
import auditLogsReducer from "./slices/auditLogsSlice";
import staffDashboardReducer from "./slices/staffDashboardSlice";
import conflictsReducer from "./slices/conflictsSlice";
import patientProfileReducer from "./slices/patientProfileSlice";

const rootReducer = combineReducers({
  auth: authReducer,
  users: usersReducer,
  providers: providerReducer,
  appointments: appointmentReducer,
  waitlist: waitlistReducer,
  documents: documentsReducer,
  intakeAppointments: intakeAppointmentReducer,
  intake: intakeReducer,
  staffDashboard: staffDashboardReducer,
  auditLogs: auditLogsReducer,
  conflicts: conflictsReducer,
  patientProfile: patientProfileReducer,
});

export default rootReducer;

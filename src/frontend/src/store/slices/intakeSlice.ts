/**
 * Redux slice for AI conversational intake (US_033)
 * Manages chat state, extracted data, and intake session
 */

import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '../index';
import type {
  IntakeState,
  ChatMessage,
  ExtractedIntakeData,
  IntakeMode,
  CategoryProgress,
  IntakeCategory,
  StartIntakeRequest,
  SendMessageRequest,
  CompleteIntakeRequest,
  ManualIntakeFormData,
  ExtractedDataItem,
} from '../../types/intake';
import {
  startIntakeSession as apiStartIntakeSession,
  sendIntakeMessage as apiSendIntakeMessage,
  completeIntake as apiCompleteIntake,
  submitManualIntake as apiSubmitManualIntake,
  switchIntakeMode as apiSwitchIntakeMode,
} from '../../api/intakeApi';

/**
 * Generate unique message ID
 */
function generateMessageId(): string {
  return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Default category progress
 */
const defaultCategoryProgress: CategoryProgress[] = [
  { category: 'chiefComplaint', completed: false, label: 'Chief Complaint' },
  { category: 'symptoms', completed: false, label: 'Symptoms' },
  { category: 'medications', completed: false, label: 'Medications' },
  { category: 'allergies', completed: false, label: 'Allergies' },
  { category: 'medicalHistory', completed: false, label: 'Medical History' },
  { category: 'familyHistory', completed: false, label: 'Family History' },
  { category: 'lifestyle', completed: false, label: 'Lifestyle' },
  { category: 'insurance', completed: false, label: 'Insurance' },
];

/**
 * Initial state for intake slice
 */
const initialState: IntakeState = {
  sessionId: null,
  appointmentId: null,
  mode: 'ai',
  status: 'idle',
  messages: [],
  isTyping: false,
  extractedData: {},
  progress: 0,
  confidenceLevel: 100,
  consecutiveFailures: 0,
  categoryProgress: [...defaultCategoryProgress],
  error: null,
};

/**
 * Async thunk to start intake session
 */
export const startIntake = createAsyncThunk<
  { sessionId: string; welcomeMessage: string },
  StartIntakeRequest,
  { state: RootState; rejectValue: string }
>(
  'intake/startIntake',
  async (request, { rejectWithValue }) => {
    try {
      const response = await apiStartIntakeSession(request);
      return {
        sessionId: response.sessionId,
        welcomeMessage: response.welcomeMessage,
      };
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to start intake session'
      );
    }
  }
);

/**
 * Async thunk to send chat message
 */
export const sendMessage = createAsyncThunk<
  {
    aiMessage: string;
    extractedData?: ExtractedDataItem[];
    progress: number;
    confidenceLevel: number;
    currentCategory: IntakeCategory;
    isComplete: boolean;
  },
  { message: string },
  { state: RootState; rejectValue: string }
>(
  'intake/sendMessage',
  async ({ message }, { getState, rejectWithValue }) => {
    try {
      const state = getState();
      const sessionId = state.intake.sessionId;

      if (!sessionId) {
        return rejectWithValue('No active intake session');
      }

      const request: SendMessageRequest = {
        sessionId,
        message,
      };

      const response = await apiSendIntakeMessage(request);
      return response;
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to send message'
      );
    }
  }
);

/**
 * Async thunk to complete intake
 */
export const completeIntake = createAsyncThunk<
  { success: boolean; intakeRecordId: string },
  void,
  { state: RootState; rejectValue: string }
>(
  'intake/completeIntake',
  async (_, { getState, rejectWithValue }) => {
    try {
      const state = getState();
      const { sessionId, extractedData } = state.intake;

      if (!sessionId) {
        return rejectWithValue('No active intake session');
      }

      const request: CompleteIntakeRequest = {
        sessionId,
        summary: {
          chiefComplaint: extractedData.chiefComplaint || '',
          symptoms: extractedData.symptoms || [],
          medications: extractedData.medications || [],
          allergies: extractedData.allergies || [],
          medicalHistory: extractedData.medicalHistory || [],
          familyHistory: extractedData.familyHistory || [],
          lifestyle: extractedData.lifestyle || {},
          insuranceInfo: extractedData.insuranceInfo,
          additionalConcerns: extractedData.additionalConcerns,
        },
      };

      const response = await apiCompleteIntake(request);
      return {
        success: response.success,
        intakeRecordId: response.intakeRecordId,
      };
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to complete intake'
      );
    }
  }
);

/**
 * Async thunk to submit manual intake form
 */
export const submitManualIntakeForm = createAsyncThunk<
  { success: boolean; intakeRecordId: string },
  ManualIntakeFormData,
  { state: RootState; rejectValue: string }
>(
  'intake/submitManualIntake',
  async (formData, { getState, rejectWithValue }) => {
    try {
      const state = getState();
      const sessionId = state.intake.sessionId;

      if (!sessionId) {
        return rejectWithValue('No active intake session');
      }

      const response = await apiSubmitManualIntake(sessionId, formData);
      return {
        success: response.success,
        intakeRecordId: response.intakeRecordId,
      };
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to submit intake'
      );
    }
  }
);

/**
 * Async thunk to switch intake mode
 */
export const switchMode = createAsyncThunk<
  { mode: IntakeMode; dataPreserved: boolean },
  IntakeMode,
  { state: RootState; rejectValue: string }
>(
  'intake/switchMode',
  async (newMode, { getState, rejectWithValue }) => {
    try {
      const state = getState();
      const sessionId = state.intake.sessionId;

      if (!sessionId) {
        return rejectWithValue('No active intake session');
      }

      const response = await apiSwitchIntakeMode(sessionId, newMode);
      return {
        mode: response.mode,
        dataPreserved: response.dataPreserved,
      };
    } catch (error) {
      return rejectWithValue(
        error instanceof Error ? error.message : 'Failed to switch mode'
      );
    }
  }
);

/**
 * Intake slice with reducers and async thunk handlers
 */
const intakeSlice = createSlice({
  name: 'intake',
  initialState,
  reducers: {
    /**
     * Reset intake state
     */
    resetIntake: () => initialState,

    /**
     * Set intake mode (without API call)
     */
    setMode: (state, action: PayloadAction<IntakeMode>) => {
      state.mode = action.payload;
    },

    /**
     * Set appointment ID
     */
    setAppointmentId: (state, action: PayloadAction<string>) => {
      state.appointmentId = action.payload;
    },

    /**
     * Add a user message to chat
     */
    addUserMessage: (state, action: PayloadAction<string>) => {
      const message: ChatMessage = {
        id: generateMessageId(),
        sender: 'user',
        content: action.payload,
        timestamp: new Date().toISOString(),
      };
      state.messages.push(message);
    },

    /**
     * Set typing indicator
     */
    setTyping: (state, action: PayloadAction<boolean>) => {
      state.isTyping = action.payload;
    },

    /**
     * Update extracted data manually
     */
    updateExtractedData: (state, action: PayloadAction<Partial<ExtractedIntakeData>>) => {
      state.extractedData = {
        ...state.extractedData,
        ...action.payload,
      };
    },

    /**
     * Clear error
     */
    clearError: (state) => {
      state.error = null;
    },

    /**
     * Reset consecutive failures counter
     */
    resetConsecutiveFailures: (state) => {
      state.consecutiveFailures = 0;
    },

    /**
     * Update manual form step progress
     */
    setManualFormStep: (state, action: PayloadAction<number>) => {
      // Calculate progress based on step (4 steps total)
      state.progress = Math.min(100, (action.payload / 4) * 100);
    },
  },
  extraReducers: (builder) => {
    // Start intake
    builder
      .addCase(startIntake.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(startIntake.fulfilled, (state, action) => {
        state.status = 'idle';
        state.sessionId = action.payload.sessionId;
        state.mode = action.meta.arg.mode;
        state.appointmentId = action.meta.arg.appointmentId;
        state.messages = [
          {
            id: generateMessageId(),
            sender: 'ai',
            content: action.payload.welcomeMessage,
            timestamp: new Date().toISOString(),
          },
        ];
        state.categoryProgress = [...defaultCategoryProgress];
        state.progress = 0;
        state.confidenceLevel = 100;
        state.consecutiveFailures = 0;
        state.error = null;
      })
      .addCase(startIntake.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload ?? 'Failed to start intake';
      });

    // Send message
    builder
      .addCase(sendMessage.pending, (state) => {
        state.isTyping = true;
        state.error = null;
      })
      .addCase(sendMessage.fulfilled, (state, action) => {
        state.isTyping = false;
        
        // Add AI response
        const aiMessage: ChatMessage = {
          id: generateMessageId(),
          sender: 'ai',
          content: action.payload.aiMessage,
          timestamp: new Date().toISOString(),
          extractedData: action.payload.extractedData,
        };
        state.messages.push(aiMessage);

        // Update progress and confidence
        state.progress = action.payload.progress;
        state.confidenceLevel = action.payload.confidenceLevel;

        // Update category progress
        const categoryIndex = state.categoryProgress.findIndex(
          (cp) => cp.category === action.payload.currentCategory
        );
        const categoryItem = state.categoryProgress[categoryIndex];
        if (categoryIndex !== -1 && categoryItem) {
          categoryItem.completed = true;
        }

        // Check confidence level for fallback (AC-4: below 70%)
        if (action.payload.confidenceLevel < 70) {
          state.consecutiveFailures++;
        } else {
          state.consecutiveFailures = 0;
        }

        // Update extracted data from response
        if (action.payload.extractedData) {
          action.payload.extractedData.forEach((item) => {
            const field = item.field as keyof ExtractedIntakeData;
            if (field === 'chiefComplaint' || field === 'additionalConcerns') {
              (state.extractedData as Record<string, unknown>)[field] = item.value;
            }
          });
        }

        // Check if complete
        if (action.payload.isComplete) {
          state.status = 'complete';
        }
      })
      .addCase(sendMessage.rejected, (state, action) => {
        state.isTyping = false;
        state.error = action.payload ?? 'Failed to send message';
        state.consecutiveFailures++;
      });

    // Complete intake
    builder
      .addCase(completeIntake.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(completeIntake.fulfilled, (state) => {
        state.status = 'complete';
        state.progress = 100;
      })
      .addCase(completeIntake.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload ?? 'Failed to complete intake';
      });

    // Submit manual intake
    builder
      .addCase(submitManualIntakeForm.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(submitManualIntakeForm.fulfilled, (state) => {
        state.status = 'complete';
        state.progress = 100;
      })
      .addCase(submitManualIntakeForm.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload ?? 'Failed to submit intake';
      });

    // Switch mode
    builder
      .addCase(switchMode.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(switchMode.fulfilled, (state, action) => {
        state.status = 'idle';
        state.mode = action.payload.mode;
      })
      .addCase(switchMode.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload ?? 'Failed to switch mode';
      });
  },
});

// Export actions
export const {
  resetIntake,
  setMode,
  setAppointmentId,
  addUserMessage,
  setTyping,
  updateExtractedData,
  clearError,
  resetConsecutiveFailures,
  setManualFormStep,
} = intakeSlice.actions;

// Export selectors
export const selectIntakeMode = (state: RootState) => state.intake.mode;
export const selectIntakeStatus = (state: RootState) => state.intake.status;
export const selectMessages = (state: RootState) => state.intake.messages;
export const selectIsTyping = (state: RootState) => state.intake.isTyping;
export const selectExtractedData = (state: RootState) => state.intake.extractedData;
export const selectProgress = (state: RootState) => state.intake.progress;
export const selectConfidenceLevel = (state: RootState) => state.intake.confidenceLevel;
export const selectConsecutiveFailures = (state: RootState) => state.intake.consecutiveFailures;
export const selectCategoryProgress = (state: RootState) => state.intake.categoryProgress;
export const selectIntakeError = (state: RootState) => state.intake.error;
export const selectSessionId = (state: RootState) => state.intake.sessionId;

// Export reducer
export default intakeSlice.reducer;

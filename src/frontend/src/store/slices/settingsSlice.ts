import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { SystemSetting, ReminderSettings } from '../../types/settings';

/**
 * Redux slice for system settings management (US_037 - AC-4).
 * Handles fetching and updating admin-configurable reminder settings.
 */

interface SettingsState {
    settings: SystemSetting[];
    loading: boolean;
    saving: boolean;
    error: string | null;
}

const initialState: SettingsState = {
    settings: [],
    loading: false,
    saving: false,
    error: null,
};

/**
 * Fetches all system settings from the API (GET /api/admin/settings).
 */
export const fetchSettings = createAsyncThunk<SystemSetting[], void, { rejectValue: string }>(
    'settings/fetchSettings',
    async (_, { rejectWithValue }) => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch('/api/admin/settings', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                const error = await response.json();
                return rejectWithValue(error.message || 'Failed to fetch settings');
            }

            const data = await response.json();
            return data as SystemSetting[];
        } catch (error) {
            return rejectWithValue('Network error');
        }
    }
);

/**
 * Updates system settings (PUT /api/admin/settings).
 */
export const updateSettings = createAsyncThunk<
    void,
    SystemSetting[],
    { rejectValue: string }
>(
    'settings/updateSettings',
    async (settings, { rejectWithValue }) => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch('/api/admin/settings', {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ settings }),
            });

            if (!response.ok) {
                const error = await response.json();
                return rejectWithValue(error.message || 'Failed to update settings');
            }
        } catch (error) {
            return rejectWithValue('Network error');
        }
    }
);

const settingsSlice = createSlice({
    name: 'settings',
    initialState,
    reducers: {
        clearError: (state) => {
            state.error = null;
        },
    },
    extraReducers: (builder) => {
        builder
            // Fetch settings
            .addCase(fetchSettings.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(fetchSettings.fulfilled, (state, action: PayloadAction<SystemSetting[]>) => {
                state.loading = false;
                state.settings = action.payload;
            })
            .addCase(fetchSettings.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload || 'Failed to fetch settings';
            })
            // Update settings
            .addCase(updateSettings.pending, (state) => {
                state.saving = true;
                state.error = null;
            })
            .addCase(updateSettings.fulfilled, (state) => {
                state.saving = false;
            })
            .addCase(updateSettings.rejected, (state, action) => {
                state.saving = false;
                state.error = action.payload || 'Failed to update settings';
            });
    },
});

export const { clearError } = settingsSlice.actions;

/**
 * Selector to parse reminder settings from SystemSettings array.
 * Converts JSON string values to typed ReminderSettings object.
 */
export const selectReminderSettings = (state: { settings: SettingsState }): ReminderSettings => {
    const { settings } = state.settings;

    const intervalsSetting = settings.find((s) => s.key === 'Reminder.Intervals');
    const smsEnabledSetting = settings.find((s) => s.key === 'Reminder.SmsEnabled');
    const emailEnabledSetting = settings.find((s) => s.key === 'Reminder.EmailEnabled');

    return {
        intervals: intervalsSetting ? JSON.parse(intervalsSetting.value) : [48, 24, 2],
        smsEnabled: smsEnabledSetting ? smsEnabledSetting.value === 'true' : true,
        emailEnabled: emailEnabledSetting ? emailEnabledSetting.value === 'true' : true,
    };
};

export default settingsSlice.reducer;

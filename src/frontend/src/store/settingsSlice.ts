import {
    createSlice,
    createAsyncThunk,
    type PayloadAction,
} from "@reduxjs/toolkit";
import type { RootState } from "./index";
import type { SystemSetting, ReminderSettings, UpdateSystemSettingsRequest } from "../types/settings";

/**
 * Settings state interface (US_037).
 */
interface SettingsState {
    settings: SystemSetting[];
    isLoading: boolean;
    error: string | null;
    isSaving: boolean;
}

const initialState: SettingsState = {
    settings: [],
    isLoading: false,
    error: null,
    isSaving: false,
};

/**
 * Async thunk to fetch system settings (US_037 - Admin API).
 */
export const fetchSettings = createAsyncThunk<
    SystemSetting[],
    void,
    { rejectValue: string }
>(
    "settings/fetchAll",
    async (_: void, { rejectWithValue }: any) => {
        try {
            const token = localStorage.getItem("token");
            const response = await fetch(
                `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/admin/settings`,
                {
                    headers: {
                        "Content-Type": "application/json",
                        ...(token && { Authorization: `Bearer ${token}` }),
                    },
                },
            );

            if (!response.ok) {
                const errorData = await response.json();
                return rejectWithValue(errorData.message || "Failed to fetch settings");
            }

            const data = await response.json();
            return data as SystemSetting[];
        } catch {
            return rejectWithValue("Network error");
        }
    },
);

/**
 * Async thunk to update system settings (US_037 - AC-4, Admin API).
 */
export const updateSettings = createAsyncThunk<
    SystemSetting[],
    UpdateSystemSettingsRequest,
    { rejectValue: string }
>(
    "settings/update",
    async (request: UpdateSystemSettingsRequest, { rejectWithValue }: any) => {
        try {
            const token = localStorage.getItem("token");
            const response = await fetch(
                `${import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"}/api/admin/settings`,
                {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        ...(token && { Authorization: `Bearer ${token}` }),
                    },
                    body: JSON.stringify(request),
                },
            );

            if (!response.ok) {
                const errorData = await response.json();
                return rejectWithValue(errorData.message || "Failed to update settings");
            }

            return request.settings; // Return updated settings
        } catch {
            return rejectWithValue("Network error");
        }
    },
);

const settingsSlice = createSlice({
    name: "settings",
    initialState,
    reducers: {
        clearError: (state: SettingsState) => {
            state.error = null;
        },
    },
    extraReducers: (builder: any) => {
        // Fetch settings
        builder.addCase(fetchSettings.pending, (state: SettingsState) => {
            state.isLoading = true;
            state.error = null;
        });
        builder.addCase(
            fetchSettings.fulfilled,
            (state: SettingsState, action: PayloadAction<SystemSetting[]>) => {
                state.isLoading = false;
                state.settings = action.payload;
            },
        );
        builder.addCase(fetchSettings.rejected, (state: SettingsState, action: any) => {
            state.isLoading = false;
            state.error = action.payload as string;
        });

        // Update settings
        builder.addCase(updateSettings.pending, (state: SettingsState) => {
            state.isSaving = true;
            state.error = null;
        });
        builder.addCase(
            updateSettings.fulfilled,
            (state: SettingsState, action: PayloadAction<SystemSetting[]>) => {
                state.isSaving = false;
                // Update local state with new values
                action.payload.forEach((updatedSetting: SystemSetting) => {
                    const index = state.settings.findIndex(
                        (s: SystemSetting) => s.key === updatedSetting.key
                    );
                    if (index !== -1) {
                        state.settings[index] = updatedSetting;
                    }
                });
            },
        );
        builder.addCase(updateSettings.rejected, (state: SettingsState, action: any) => {
            state.isSaving = false;
            state.error = action.payload as string;
        });
    },
});

export const { clearError } = settingsSlice.actions;

export default settingsSlice.reducer;

/**
 * Selector to parse and return reminder settings from SystemSettings array (US_037).
 */
export const selectReminderSettings = (
    state: RootState
): ReminderSettings | null => {
    const { settings } = state.settings;

    const intervalsSetting = settings.find((s: SystemSetting) => s.key === "Reminder.Intervals");
    const smsEnabledSetting = settings.find((s: SystemSetting) => s.key === "Reminder.SmsEnabled");
    const emailEnabledSetting = settings.find((s: SystemSetting) => s.key === "Reminder.EmailEnabled");

    if (!intervalsSetting) {
        return null;
    }

    try {
        const intervals: number[] = JSON.parse(intervalsSetting.value);
        const smsEnabled = smsEnabledSetting?.value.toLowerCase() === "true";
        const emailEnabled = emailEnabledSetting?.value.toLowerCase() === "true";

        return {
            intervals,
            smsEnabled,
            emailEnabled,
        };
    } catch {
        return null;
    }
};

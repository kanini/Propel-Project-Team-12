/**
 * Unit tests for appointmentSlice Redux state (US_024)
 * Tests state transitions and async thunks
 */

import { describe, it, expect, beforeEach } from 'vitest';
import { configureStore } from '@reduxjs/toolkit';
import appointmentReducer, {
    setSelectedProvider,
    setSelectedDate,
    setSelectedTimeSlot,
    setVisitReason,
    setEnablePreferredSwap,
    nextStep,
    previousStep,
    resetBooking,
} from '../../../store/slices/appointmentSlice';
import type { TimeSlot } from '../../../types/appointment';

type TestStore = ReturnType<typeof configureStore<{ appointments: ReturnType<typeof appointmentReducer> }>>;

describe('appointmentSlice', () => {
    let store: TestStore;

    beforeEach(() => {
        store = configureStore({
            reducer: {
                appointments: appointmentReducer,
            },
        });
    });

    it('has correct initial state', () => {
        const state = store.getState().appointments;

        expect(state.currentStep).toBe(1);
        expect(state.selectedProviderId).toBeNull();
        expect(state.selectedDate).toBeNull();
        expect(state.selectedTimeSlot).toBeNull();
        expect(state.visitReason).toBe('');
        expect(state.enablePreferredSwap).toBe(false);
    });

    it('sets selected provider and moves to step 2', () => {
        store.dispatch(
            setSelectedProvider({
                id: 'provider-1',
                name: 'Dr. Sarah Chen',
                specialty: 'Family Medicine',
            })
        );

        const state = store.getState().appointments;
        expect(state.selectedProviderId).toBe('provider-1');
        expect(state.selectedProviderName).toBe('Dr. Sarah Chen');
        expect(state.selectedProviderSpecialty).toBe('Family Medicine');
        expect(state.currentStep).toBe(2);
    });

    it('sets selected date', () => {
        store.dispatch(setSelectedDate('2026-03-21'));

        const state = store.getState().appointments;
        expect(state.selectedDate).toBe('2026-03-21');
    });

    it('sets selected time slot', () => {
        const slot: TimeSlot = {
            id: 'slot-1',
            providerId: 'provider-1',
            startTime: '2026-03-21T10:00:00Z',
            endTime: '2026-03-21T10:30:00Z',
            status: 'available',
        };

        store.dispatch(setSelectedTimeSlot(slot));

        const state = store.getState().appointments;
        expect(state.selectedTimeSlot).toEqual(slot);
    });

    it('sets visit reason', () => {
        store.dispatch(setVisitReason('Annual checkup'));

        const state = store.getState().appointments;
        expect(state.visitReason).toBe('Annual checkup');
    });

    it('toggles preferred swap', () => {
        store.dispatch(setEnablePreferredSwap(true));

        let state = store.getState().appointments;
        expect(state.enablePreferredSwap).toBe(true);

        store.dispatch(setEnablePreferredSwap(false));

        state = store.getState().appointments;
        expect(state.enablePreferredSwap).toBe(false);
        expect(state.preferredSlotId).toBeNull();
    });

    it('navigates to next step', () => {
        store.dispatch(nextStep());

        const state = store.getState().appointments;
        expect(state.currentStep).toBe(2);
    });

    it('navigates to previous step', () => {
        store.dispatch(nextStep());
        store.dispatch(nextStep());
        store.dispatch(previousStep());

        const state = store.getState().appointments;
        expect(state.currentStep).toBe(2);
    });

    it('does not go below step 1', () => {
        store.dispatch(previousStep());

        const state = store.getState().appointments;
        expect(state.currentStep).toBe(1);
    });

    it('does not go above step 4', () => {
        store.dispatch(nextStep());
        store.dispatch(nextStep());
        store.dispatch(nextStep());
        store.dispatch(nextStep()); // Try to go past 4

        const state = store.getState().appointments;
        expect(state.currentStep).toBe(4);
    });

    it('resets booking state', () => {
        store.dispatch(
            setSelectedProvider({
                id: 'provider-1',
                name: 'Dr. Sarah Chen',
                specialty: 'Family Medicine',
            })
        );
        store.dispatch(setSelectedDate('2026-03-21'));
        store.dispatch(setVisitReason('Annual checkup'));

        store.dispatch(resetBooking());

        const state = store.getState().appointments;
        expect(state.currentStep).toBe(1);
        expect(state.selectedProviderId).toBeNull();
        expect(state.selectedDate).toBeNull();
        expect(state.visitReason).toBe('');
    });
});

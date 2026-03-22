/**
 * Unit tests for VisitReasonForm Component (US_024)
 * Tests form validation and user input (UXR-601)
 */

import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import appointmentReducer from '../../../store/slices/appointmentSlice';
import { VisitReasonForm } from '../../../components/appointments/VisitReasonForm';

// Helper to create test store
const createTestStore = () => {
    return configureStore({
        reducer: {
            appointments: appointmentReducer,
        },
    });
};

describe('VisitReasonForm', () => {
    it('renders visit reason input', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        expect(screen.getByLabelText(/Reason for visit/i)).toBeInTheDocument();
    });

    it('shows required field message', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        expect(screen.getByText('Required field')).toBeInTheDocument();
    });

    it('shows error when field is blurred empty', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        const input = screen.getByLabelText(/Reason for visit/i);

        // Blur without entering value
        fireEvent.blur(input);

        expect(screen.getByText('Visit reason is required')).toBeInTheDocument();
    });

    it('updates character count as user types', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        const input = screen.getByLabelText(/Reason for visit/i);

        fireEvent.change(input, { target: { value: 'Annual checkup' } });

        expect(screen.getByText('15/200')).toBeInTheDocument();
    });

    it('shows error when exceeding max length', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        const input = screen.getByLabelText(/Reason for visit/i);

        // Enter a very long string
        const longString = 'a'.repeat(201);
        fireEvent.change(input, { target: { value: longString } });
        fireEvent.blur(input);

        expect(
            screen.getByText(/Visit reason must be 200 characters or less/i)
        ).toBeInTheDocument();
    });

    it('renders preferred swap toggle', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        expect(
            screen.getByLabelText(/Notify me if an earlier slot becomes available/i)
        ).toBeInTheDocument();
    });

    it('shows preferred swap notice when enabled', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        const checkbox = screen.getByLabelText(
            /Notify me if an earlier slot becomes available/i
        );

        fireEvent.click(checkbox);

        expect(screen.getByText(/Preferred Slot Swap/i)).toBeInTheDocument();
        expect(screen.getByText(/Coming soon/i)).toBeInTheDocument();
    });

    it('has proper ARIA attributes for validation', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        const input = screen.getByLabelText(/Reason for visit/i);

        expect(input).toHaveAttribute('aria-describedby');
        expect(input).toHaveAttribute('required');
    });

    it('renders visit type select', () => {
        const store = createTestStore();

        render(
            <Provider store={store}>
                <VisitReasonForm />
            </Provider>
        );

        expect(screen.getByLabelText(/Visit type/i)).toBeInTheDocument();
    });
});

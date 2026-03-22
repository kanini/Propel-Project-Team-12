/**
 * Unit tests for ProgressIndicator Component (US_024)
 * Tests step display and state transitions
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ProgressIndicator } from '../../../components/appointments/ProgressIndicator';

describe('ProgressIndicator', () => {
    it('renders all 4 steps', () => {
        render(<ProgressIndicator currentStep={1} />);

        expect(screen.getByText('Provider')).toBeInTheDocument();
        expect(screen.getByText('Date & Time')).toBeInTheDocument();
        expect(screen.getByText('Details')).toBeInTheDocument();
        expect(screen.getByText('Confirm')).toBeInTheDocument();
    });

    it('highlights current step correctly', () => {
        render(<ProgressIndicator currentStep={2} />);

        const dateTimeLabel = screen.getAllByText('Date & Time')[0]; // Get first occurrence
        expect(dateTimeLabel).toHaveClass('text-primary-500', 'font-medium');
    });

    it('shows completed steps with checkmark', () => {
        const { container } = render(<ProgressIndicator currentStep={3} />);

        // First two steps should have checkmarks (SVG paths)
        const checkmarks = container.querySelectorAll('path[d*="M5 13l4 4L19 7"]');
        expect(checkmarks.length).toBeGreaterThanOrEqual(2);
    });

    it('shows future steps as inactive', () => {
        render(<ProgressIndicator currentStep={1} />);

        const confirmLabel = screen.getAllByText('Confirm')[0];
        expect(confirmLabel).toHaveClass('text-neutral-500');
    });

    it('has proper ARIA attributes', () => {
        const { container } = render(<ProgressIndicator currentStep={2} />);

        const nav = container.querySelector('[role="navigation"]');
        expect(nav).toBeInTheDocument();
    });
});

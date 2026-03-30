/**
 * Empty Dashboard Component for US_068 - Staff Dashboard
 * Empty state when no appointments scheduled (UXR-605)
 */

import { useNavigate } from 'react-router-dom';

export function EmptyDashboard() {
  const navigate = useNavigate();

  return (
    <div className="text-center py-16">
      <div className="text-6xl mb-4">📋</div>
      <h2 className="text-xl font-semibold text-neutral-900 mb-2">
        No appointments scheduled today
      </h2>
      <p className="text-neutral-600 mb-6">
        Use Walk-in Booking to add patients to the queue
      </p>
      <button
        onClick={() => navigate('/staff/walk-in')}
        className="bg-primary-600 hover:bg-primary-700 text-white px-6 py-3 rounded-lg font-medium shadow-sm hover:shadow-md transition-all"
      >
        Book Walk-in Appointment →
      </button>
    </div>
  );
}

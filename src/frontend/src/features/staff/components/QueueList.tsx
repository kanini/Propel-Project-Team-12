/**
 * QueueList Component for US_030 - Queue Management.
 * Table display of queue with patient rows.
 */

import { QueuePatientRow } from "./QueuePatientRow";
import type { QueuePatient } from "../../../utils/queueHelpers";

interface QueueListProps {
  queue: QueuePatient[];
  onTogglePriority: (patientId: string, isPriority: boolean) => Promise<void>;
}

/**
 * Queue list table component
 */
export function QueueList({ queue, onTogglePriority }: QueueListProps) {
  return (
    <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-neutral-200">
          <thead className="bg-neutral-50">
            <tr>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                #
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Patient Name
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Appointment Type
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Provider
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Arrival Time
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Wait Time
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Priority
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 uppercase tracking-wider"
              >
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-neutral-0 divide-y divide-neutral-200">
            {queue.map((patient) => (
              <QueuePatientRow
                key={patient.id}
                patient={patient}
                onTogglePriority={onTogglePriority}
              />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

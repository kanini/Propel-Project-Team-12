/**
 * Custom hook for Pusher conflict notifications (US_048, AC2).
 * Subscribes to patient channel and dispatches Redux actions for real-time conflict alerts.
 * Only receives notifications for Critical severity conflicts.
 */

import { useEffect, useRef } from "react";
import { useAppDispatch } from "../store/hooks";
import Pusher from "pusher-js";
import {
  fetchConflictSummary,
  fetchPatientConflicts,
} from "../store/slices/conflictsSlice";

interface UsePusherConflictsOptions {
  patientId: number | null;
  enabled: boolean;
}

export function usePusherConflicts({
  patientId,
  enabled,
}: UsePusherConflictsOptions) {
  const dispatch = useAppDispatch();
  const pusherRef = useRef<Pusher | null>(null);
  const channelRef = useRef<ReturnType<Pusher["subscribe"]> | null>(null);
  const liveRegionRef = useRef<HTMLDivElement | null>(null);
  const toastRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!enabled || !patientId) return;

    // Get Pusher configuration from environment
    const pusherKey = import.meta.env.VITE_PUSHER_KEY;
    const pusherCluster = import.meta.env.VITE_PUSHER_CLUSTER || "us2";

    if (!pusherKey) {
      console.warn(
        "Pusher key not configured. Conflict notifications will not be real-time.",
      );
      return;
    }

    // Create ARIA live region for critical conflict announcements
    if (!liveRegionRef.current) {
      liveRegionRef.current = document.createElement("div");
      liveRegionRef.current.setAttribute("aria-live", "assertive"); // Critical conflicts use assertive
      liveRegionRef.current.setAttribute("aria-atomic", "true");
      liveRegionRef.current.className = "sr-only"; // Screen reader only
      document.body.appendChild(liveRegionRef.current);
    }

    // Initialize Pusher
    const pusher = new Pusher(pusherKey, {
      cluster: pusherCluster,
      forceTLS: true,
    });
    pusherRef.current = pusher;

    // Subscribe to patient channel
    const channelName = `private-patient-${patientId}`;
    const channel = pusher.subscribe(channelName);
    channelRef.current = channel;

    console.log(`Subscribed to Pusher channel: ${channelName}`);

    // Bind to critical-conflict-detected event
    channel.bind(
      "critical-conflict-detected",
      (data: {
        conflictId: string;
        severity: string;
        entityType: string;
        conflictType: string;
        description: string;
        detectedAt: string;
      }) => {
        console.log("Critical conflict detected:", data.conflictId);

        // Show toast notification
        showToastNotification(data.description, data.entityType);

        // Announce for screen readers
        if (liveRegionRef.current) {
          liveRegionRef.current.textContent = `Critical conflict detected: ${data.description}`;
        }

        // Refetch conflicts and summary to get latest data
        dispatch(fetchConflictSummary(patientId));
        dispatch(
          fetchPatientConflicts({
            patientId,
            unresolvedOnly: true,
            page: 1,
            pageSize: 10,
          }),
        );
      },
    );

    // Bind to patient-profile-updated event (re-aggregation completed)
    channel.bind(
      "patient-profile-updated",
      (data: {
        patientId: string;
        profileCompleteness: number;
        conflictsDetected: number;
        hasUnresolvedConflicts: boolean;
      }) => {
        console.log("Patient profile updated:", data);

        // Refetch summary if conflicts were detected
        if (data.conflictsDetected > 0) {
          dispatch(fetchConflictSummary(patientId));
        }
      },
    );

    // Cleanup function
    return () => {
      if (channelRef.current) {
        channelRef.current.unbind_all();
        channelRef.current.unsubscribe();
      }
      if (pusherRef.current) {
        pusherRef.current.disconnect();
      }
      if (liveRegionRef.current) {
        document.body.removeChild(liveRegionRef.current);
        liveRegionRef.current = null;
      }
      if (toastRef.current) {
        document.body.removeChild(toastRef.current);
        toastRef.current = null;
      }
    };
  }, [enabled, patientId, dispatch]);

  // Show toast notification for critical conflicts
  const showToastNotification = (description: string, entityType: string) => {
    // Create toast container if it doesn't exist
    if (!toastRef.current) {
      toastRef.current = document.createElement("div");
      toastRef.current.className = "fixed top-4 right-4 z-50";
      document.body.appendChild(toastRef.current);
    }

    // Create toast element
    const toast = document.createElement("div");
    toast.className =
      "bg-red-600 text-white px-4 py-3 rounded-lg shadow-lg max-w-md animate-slide-in";
    toast.innerHTML = `
      <div class="flex items-start gap-3">
        <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
        <div class="flex-1 min-w-0">
          <p class="font-semibold">Critical ${entityType} Conflict Detected</p>
          <p class="text-sm text-red-100 mt-1">Review required for patient safety</p>
        </div>
        <button onclick="this.parentElement.parentElement.remove()" class="text-red-200 hover:text-white focus:outline-none">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    `;

    toastRef.current.appendChild(toast);

    // Auto-remove toast after 5 seconds
    setTimeout(() => {
      toast.classList.add("animate-slide-out");
      setTimeout(() => toast.remove(), 300);
    }, 5000);
  };
}

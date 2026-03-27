/**
 * Custom hook for Pusher queue real-time updates (US_030, AC-2).
 * Manages WebSocket connection with 30-second polling fallback.
 */

import { useEffect, useState, useCallback, useRef } from "react";
import Pusher from "pusher-js";
import type { QueuePatient } from "../utils/queueHelpers";

/**
 * Pusher connection states
 */
export type ConnectionState = "connected" | "connecting" | "disconnected";

/**
 * Queue event types from Pusher
 */
export interface QueueEvent {
  type: "patient-added" | "patient-removed" | "priority-changed";
  patient: QueuePatient;
}

/**
 * Pusher queue hook options
 */
interface UsePusherQueueOptions {
  /**
   * Callback when patient is added to queue
   */
  onPatientAdded: (patient: QueuePatient) => void;
  /**
   * Callback when patient is removed from queue
   */
  onPatientRemoved: (patientId: string) => void;
  /**
   * Callback when patient priority changes
   */
  onPriorityChanged: (patientId: string, isPriority: boolean) => void;
  /**
   * Fallback polling function to fetch latest queue data
   */
  fetchQueueFallback: () => Promise<void>;
}

/**
 * Custom hook for Pusher queue management with fallback polling
 */
export function usePusherQueue(options: UsePusherQueueOptions) {
  const {
    onPatientAdded,
    onPatientRemoved,
    onPriorityChanged,
    fetchQueueFallback,
  } = options;

  const [connectionState, setConnectionState] =
    useState<ConnectionState>("connecting");
  const [isUsingFallback, setIsUsingFallback] = useState(false);
  const [reconnectAttempts, setReconnectAttempts] = useState(0);

  const pusherRef = useRef<Pusher | null>(null);
  const channelRef = useRef<ReturnType<Pusher['subscribe']> | null>(null);
  const fallbackIntervalRef = useRef<number | null>(null);

  /**
   * Start fallback polling (30-second interval)
   */
  const startFallbackPolling = useCallback(() => {
    // Prevent duplicate intervals
    if (fallbackIntervalRef.current) {
      return;
    }

    console.log("Starting fallback polling (30s interval)");

    // Initial fetch
    fetchQueueFallback();

    // Set up 30-second interval
    fallbackIntervalRef.current = setInterval(() => {
      console.log("Fallback polling: fetching queue data");
      fetchQueueFallback();
    }, 30000); // 30 seconds
  }, [fetchQueueFallback]);

  /**
   * Initialize Pusher connection
   */
  const initializePusher = useCallback(() => {
    try {
      // Get Pusher configuration from environment variables
      const pusherKey = import.meta.env.VITE_PUSHER_KEY;
      const pusherCluster = import.meta.env.VITE_PUSHER_CLUSTER || "us2";

      if (!pusherKey) {
        console.warn("Pusher key not configured. Using fallback polling only.");
        setIsUsingFallback(true);
        setConnectionState("disconnected");
        return;
      }

      // Create Pusher instance
      const pusher = new Pusher(pusherKey, {
        cluster: pusherCluster,
        forceTLS: true,
      });

      pusherRef.current = pusher;

      // Subscribe to queue-updates channel
      const channel = pusher.subscribe("queue-updates");
      channelRef.current = channel;

      // Bind to Pusher connection state events
      pusher.connection.bind("connected", () => {
        console.log("Pusher connected");
        setConnectionState("connected");
        setIsUsingFallback(false);
        setReconnectAttempts(0);

        // Clear fallback polling when connected
        if (fallbackIntervalRef.current) {
          clearInterval(fallbackIntervalRef.current);
          fallbackIntervalRef.current = null;
        }
      });

      pusher.connection.bind("connecting", () => {
        console.log("Pusher connecting...");
        setConnectionState("connecting");
      });

      pusher.connection.bind("disconnected", () => {
        console.log("Pusher disconnected");
        setConnectionState("disconnected");
        setIsUsingFallback(true);
        setReconnectAttempts((prev) => prev + 1);

        // Start fallback polling (30 seconds interval)
        startFallbackPolling();
      });

      pusher.connection.bind("error", (error: unknown) => {
        console.error("Pusher connection error:", error);
        setConnectionState("disconnected");
        setIsUsingFallback(true);

        // Start fallback polling on error
        startFallbackPolling();
      });

      // Bind to queue event channels
      channel.bind("patient-added", (data: QueuePatient) => {
        console.log("Patient added event:", data);
        onPatientAdded(data);
      });

      channel.bind("patient-removed", (data: { patientId: string }) => {
        console.log("Patient removed event:", data);
        onPatientRemoved(data.patientId);
      });

      channel.bind(
        "priority-changed",
        (data: { patientId: string; isPriority: boolean }) => {
          console.log("Priority changed event:", data);
          onPriorityChanged(data.patientId, data.isPriority);
        },
      );
    } catch (error) {
      console.error("Failed to initialize Pusher:", error);
      setConnectionState("disconnected");
      setIsUsingFallback(true);
      startFallbackPolling();
    }
  }, [onPatientAdded, onPatientRemoved, onPriorityChanged, startFallbackPolling]);

  /**
   * Manual retry connection
   */
  const retryConnection = useCallback(() => {
    if (pusherRef.current) {
      console.log("Retrying Pusher connection...");
      pusherRef.current.connect();
    }
  }, []);

  /**
   * Initialize Pusher on mount
   */
  useEffect(() => {
    initializePusher(); // eslint-disable-line react-hooks/set-state-in-effect -- Initializing Pusher connection on mount is intentional

    // Cleanup on unmount
    return () => {
      if (channelRef.current) {
        channelRef.current.unbind_all();
        channelRef.current.unsubscribe();
      }
      if (pusherRef.current) {
        pusherRef.current.disconnect();
      }
      if (fallbackIntervalRef.current) {
        clearInterval(fallbackIntervalRef.current);
      }
    };
  }, [initializePusher]);

  return {
    connectionState,
    isUsingFallback,
    reconnectAttempts,
    retryConnection,
  };
}

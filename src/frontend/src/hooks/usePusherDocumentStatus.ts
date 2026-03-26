/**
 * Custom hook for Pusher document status tracking (US_044, AC2).
 * Subscribes to patient document channel and dispatches Redux actions for real-time status updates.
 * Announces status changes via ARIA live region for accessibility (UXR-207).
 */

import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import Pusher from 'pusher-js';
import { updateDocumentStatus } from '../store/documentsSlice';

interface UsePusherDocumentStatusOptions {
  userId: string | null;
  enabled: boolean;
}

export function usePusherDocumentStatus({ userId, enabled }: UsePusherDocumentStatusOptions) {
  const dispatch = useDispatch();
  const pusherRef = useRef<Pusher | null>(null);
  const channelRef = useRef<any>(null);
  const liveRegionRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!enabled || !userId) return;

    // Get Pusher configuration from environment
    const pusherKey = import.meta.env.VITE_PUSHER_KEY;
    const pusherCluster = import.meta.env.VITE_PUSHER_CLUSTER || 'us2';

    if (!pusherKey) {
      console.warn('Pusher key not configured. Document status will use polling fallback.');
      return;
    }

    // Create ARIA live region for status announcements (UXR-207)
    if (!liveRegionRef.current) {
      liveRegionRef.current = document.createElement('div');
      liveRegionRef.current.setAttribute('aria-live', 'polite');
      liveRegionRef.current.setAttribute('aria-atomic', 'true');
      liveRegionRef.current.className = 'sr-only'; // Screen reader only
      document.body.appendChild(liveRegionRef.current);
    }

    // Initialize Pusher
    const pusher = new Pusher(pusherKey, {
      cluster: pusherCluster,
      forceTLS: true,
    });
    pusherRef.current = pusher;

    // Subscribe to patient document channel
    const channelName = `patient-${userId}-documents`;
    const channel = pusher.subscribe(channelName);
    channelRef.current = channel;

    console.log(`Subscribed to Pusher channel: ${channelName}`);

    // Bind to processing-started event
    channel.bind('processing-started', (data: {
      documentId: string;
      fileName: string;
      status: string;
      timestamp: string;
    }) => {
      console.log('Document processing started:', data.documentId);
      dispatch(updateDocumentStatus({
        documentId: data.documentId,
        status: 'Processing',
      }));

      // Announce status change for accessibility
      if (liveRegionRef.current) {
        liveRegionRef.current.textContent = `${data.fileName} is now Processing`;
      }
    });

    // Bind to processing-completed event
    channel.bind('processing-completed', (data: {
      documentId: string;
      fileName: string;
      status: string;
      processingTimeMs: number;
      timestamp: string;
    }) => {
      console.log('Document processing completed:', data.documentId);
      dispatch(updateDocumentStatus({
        documentId: data.documentId,
        status: 'Completed',
        processedAt: data.timestamp,
        processingTimeMs: data.processingTimeMs,
      }));

      // Announce status change for accessibility
      if (liveRegionRef.current) {
        liveRegionRef.current.textContent = `${data.fileName} is now Completed`;
      }
    });

    // Bind to processing-failed event
    channel.bind('processing-failed', (data: {
      documentId: string;
      fileName: string;
      status: string;
      errorMessage: string;
      timestamp: string;
    }) => {
      console.log('Document processing failed:', data.documentId);
      dispatch(updateDocumentStatus({
        documentId: data.documentId,
        status: 'Failed',
        errorMessage: data.errorMessage,
        processedAt: data.timestamp,
      }));

      // Announce status change for accessibility
      if (liveRegionRef.current) {
        liveRegionRef.current.textContent = `${data.fileName} processing Failed. ${data.errorMessage}`;
      }
    });

    // Cleanup on unmount
    return () => {
      if (channelRef.current) {
        channelRef.current.unbind_all();
        pusher.unsubscribe(channelName);
      }
      if (pusherRef.current) {
        pusher.disconnect();
      }
      if (liveRegionRef.current && document.body.contains(liveRegionRef.current)) {
        document.body.removeChild(liveRegionRef.current);
        liveRegionRef.current = null;
      }
    };
  }, [userId, enabled, dispatch]);
}

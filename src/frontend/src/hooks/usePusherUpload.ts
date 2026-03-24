/**
 * Custom hook for Pusher upload progress tracking (US_042, AC-2).
 * Subscribes to upload-specific channel and dispatches Redux actions for real-time updates.
 */

import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import Pusher from 'pusher-js';
import { updateUploadProgress, completeUpload, setUploadError, pauseUpload } from '../store/documentsSlice';

interface UsePusherUploadOptions {
  uploadId: string;
  pusherChannel: string | null;
  enabled: boolean;
}

export function usePusherUpload({ uploadId, pusherChannel, enabled }: UsePusherUploadOptions) {
  const dispatch = useDispatch();
  const pusherRef = useRef<Pusher | null>(null);
  const channelRef = useRef<any>(null);

  useEffect(() => {
    if (!enabled || !pusherChannel) return;

    // Get Pusher configuration from environment
    const pusherKey = import.meta.env.VITE_PUSHER_KEY;
    const pusherCluster = import.meta.env.VITE_PUSHER_CLUSTER || 'us2';

    if (!pusherKey) {
      console.warn('Pusher key not configured. Upload progress will use polling fallback.');
      return;
    }

    // Initialize Pusher
    const pusher = new Pusher(pusherKey, {
      cluster: pusherCluster,
      forceTLS: true,
    });
    pusherRef.current = pusher;

    // Subscribe to upload channel
    const channel = pusher.subscribe(pusherChannel);
    channelRef.current = channel;

    // Bind to upload progress events
    channel.bind('chunk-uploaded', (data: {
      chunksReceived: number;
      totalChunks: number;
      percentComplete: number;
      status: string;
    }) => {
      dispatch(updateUploadProgress({
        uploadId,
        chunksReceived: data.chunksReceived,
        percentComplete: data.percentComplete,
      }));
    });

    // Bind to upload complete event
    channel.bind('upload-complete', (data: {
      documentId: string;
      fileName: string;
      fileSize: number;
      status: string;
    }) => {
      dispatch(completeUpload({
        uploadId,
        documentId: data.documentId,
      }));
    });

    // Bind to upload failed event
    channel.bind('upload-failed', (data: { error: string; status: string }) => {
      dispatch(setUploadError({
        uploadId,
        error: data.error,
      }));
    });

    // Bind to upload paused event (network interruption)
    channel.bind('upload-paused', () => {
      dispatch(pauseUpload({ uploadId }));
    });

    // Cleanup on unmount
    return () => {
      if (channelRef.current) {
        channelRef.current.unbind_all();
        pusher.unsubscribe(pusherChannel);
      }
      if (pusherRef.current) {
        pusherRef.current.disconnect();
      }
    };
  }, [uploadId, pusherChannel, enabled, dispatch]);
}

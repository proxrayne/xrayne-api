import { useCallback, useEffect, useRef, useState } from "react";

import { api, getAuthorizationToken } from "@core/api/instance";

interface StreamPullingOptions {
  withAuth?: boolean;
  reconnectDelay?: number;
}

interface StreamPullingResult<T> {
  data: T | null;
  connect: () => void;
  disconnect: () => void;
  isConnected: boolean;
}

export function useStreamPulling<T extends Object>(
  path: string | null,
  { withAuth = true, reconnectDelay = 3_000 }: StreamPullingOptions = {},
): StreamPullingResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const eventSourceRef = useRef<EventSource | null>(null);
  const reconnectTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(
    null,
  );
  const isDisposedRef = useRef(false);
  const shouldReconnectRef = useRef(false);
  const connectRef = useRef<() => void>(() => {});

  const closeStream = useCallback(() => {
    eventSourceRef.current?.close();
    eventSourceRef.current = null;
    setIsConnected(false);
  }, []);

  const clearReconnect = useCallback(() => {
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
      reconnectTimeoutRef.current = null;
    }
  }, []);

  const disconnect = useCallback(() => {
    shouldReconnectRef.current = false;
    clearReconnect();
    closeStream();
  }, [clearReconnect, closeStream]);

  const createUrl = useCallback(() => {
    if (!path) {
      return null;
    }

    const url = new URL(
      `${api.defaults.baseURL}/${path}`,
      window.location.origin,
    );
    const authToken = getAuthorizationToken();

    if (withAuth && authToken) {
      url.searchParams.set("access_token", authToken);
    }

    return url.toString();
  }, [path, withAuth]);

  const scheduleReconnect = useCallback(() => {
    if (
      isDisposedRef.current ||
      !shouldReconnectRef.current ||
      reconnectTimeoutRef.current
    ) {
      return;
    }

    closeStream();
    reconnectTimeoutRef.current = setTimeout(() => {
      reconnectTimeoutRef.current = null;
      connectRef.current();
    }, reconnectDelay);
  }, [closeStream, reconnectDelay]);

  const connect = useCallback(() => {
    const url = createUrl();
    if (!url || isDisposedRef.current) {
      return;
    }

    shouldReconnectRef.current = true;
    clearReconnect();
    closeStream();

    const eventSource = new EventSource(url);
    eventSourceRef.current = eventSource;

    eventSource.addEventListener("open", () => {
      clearReconnect();
      setIsConnected(true);
    });
    eventSource.addEventListener("error", scheduleReconnect);
    eventSource.addEventListener("message", (ev) => {
      setData(JSON.parse(ev.data));
    });
  }, [clearReconnect, closeStream, createUrl, scheduleReconnect]);

  useEffect(() => {
    connectRef.current = connect;
  }, [connect]);

  useEffect(() => {
    isDisposedRef.current = false;

    if (!path) {
      setData(null);
      disconnect();
      return;
    }

    connect();

    return () => {
      isDisposedRef.current = true;
      disconnect();
    };
  }, [connect, disconnect, path]);

  return { data, connect, disconnect, isConnected };
}

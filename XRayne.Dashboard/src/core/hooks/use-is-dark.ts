import { useSyncExternalStore } from "react";

const darkSchemeQuery = "(prefers-color-scheme: dark)";

function getSnapshot() {
  if (typeof window === "undefined") {
    return false;
  }

  return window.matchMedia(darkSchemeQuery).matches;
}

function subscribe(onStoreChange: () => void) {
  if (typeof window === "undefined") {
    return () => {};
  }

  const mediaQuery = window.matchMedia(darkSchemeQuery);

  mediaQuery.addEventListener("change", onStoreChange);

  return () => {
    mediaQuery.removeEventListener("change", onStoreChange);
  };
}

export function useIsDark() {
  return useSyncExternalStore(subscribe, getSnapshot, () => false);
}

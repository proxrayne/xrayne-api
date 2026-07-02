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

/**
 * Returns the current dark-mode state from the document theme or system preference.
 *
 * Reacts to `class` and `data-theme` changes on the root element and to
 * `prefers-color-scheme` changes when the document does not force a theme.
 */
export function useIsDark() {
  return useSyncExternalStore(subscribe, getSnapshot, () => false);
}

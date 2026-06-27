import { useEffect, useLayoutEffect, useRef, useState } from "react";

export function useFakeProgress(isRouteLoading: boolean) {
  const [progress, setProgress] = useState(0);
  const [isVisible, setIsVisible] = useState(false);
  const timerRef = useRef<NodeJS.Timeout>(null);

  useEffect(() => {
    if (isRouteLoading) {
      setIsVisible(true);
      setProgress(5);

      timerRef.current = setInterval(() => {
        setProgress((prev) => {
          if (prev >= 98) {
            return prev;
          }

          return Math.min(prev + Math.random() * ((100 - prev) * 0.1), 98);
        });
      }, 200);

      return;
    }

    setProgress(100);
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }

    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
    };
  }, [isRouteLoading]);

  const fullProgress = progress === 100;

  useLayoutEffect(() => {
    if (!fullProgress) {
      return;
    }

    const timeout = setTimeout(() => setIsVisible(false), 500);

    return () => {
      clearTimeout(timeout);
    };
  }, [fullProgress]);

  return [progress, isVisible] as const;
}

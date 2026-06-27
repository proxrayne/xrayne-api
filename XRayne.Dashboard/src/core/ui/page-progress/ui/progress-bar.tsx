import { useSpring, useTransform, motion } from "framer-motion";
import { useLayoutEffect } from "react";

function ProgressBar({ progress }: { progress: number }) {
  const smoothProgress = useSpring(progress, {
    damping: 20,
    stiffness: 100,
  });

  const width = useTransform(smoothProgress, (latest) => `${latest}%`);

  useLayoutEffect(() => {
    smoothProgress.set(progress);
  }, [progress]);

  return (
    <motion.div className="h-full bg-accent rounded-full" style={{ width }} />
  );
}

export default ProgressBar;

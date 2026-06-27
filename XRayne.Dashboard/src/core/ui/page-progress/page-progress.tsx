import { useNavigation } from "react-router";
import { AnimatePresence, motion } from "framer-motion";

import ProgressBar from "./ui/progress-bar";
import { useFakeProgress } from "./lib/hooks";

function PageProgress() {
  const navigation = useNavigation();
  const [progress, isVisible] = useFakeProgress(navigation.state !== "idle");

  return (
    <AnimatePresence mode="popLayout">
      {isVisible && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.2, ease: "easeInOut" }}
          className="fixed top-0 left-0 right-0 h-0.5 z-50"
        >
          <ProgressBar progress={progress} />
        </motion.div>
      )}
    </AnimatePresence>
  );
}

export default PageProgress;

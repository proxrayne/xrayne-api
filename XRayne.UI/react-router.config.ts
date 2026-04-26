import type { Config } from "@react-router/dev/config";

export default {
  appDirectory: "src",
  future: {
    v8_middleware: true,
  },
  ssr: false,
} satisfies Config;

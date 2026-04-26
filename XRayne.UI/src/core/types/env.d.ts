/// <reference types="vite-plugin-svgr/client" />
/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_BOT_USERNAME: string;
  readonly VITE_DOMAIN: string;
  readonly VITE_API_DOMAIN: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

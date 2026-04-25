import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import React from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";

const queryClient = new QueryClient();

function App() {
  return (
    <main className="shell">
      <section className="header">
        <div>
          <p className="eyebrow">XRayne.Node</p>
          <h1>Панель управления нодой</h1>
        </div>
      </section>

      <section className="status-grid">
        <article className="status-card">
          <span>Нода</span>
          <strong>XRayne</strong>
        </article>
        <article className="status-card">
          <span>Режим</span>
          <strong>Standalone</strong>
        </article>
        <article className="status-card">
          <span>Xray</span>
          <strong>Pending</strong>
        </article>
        <article className="status-card">
          <span>Версия</span>
          <strong>Pending</strong>
        </article>
      </section>
    </main>
  );
}

createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </React.StrictMode>
);

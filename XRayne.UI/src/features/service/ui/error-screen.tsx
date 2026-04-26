import { ExclamationCircleIcon } from "@heroicons/react/16/solid";
import type { ReactNode } from "react";

interface Props {
  title: string;
  details: string;
  status: number;
  children?: ReactNode;
}

function ErrorScreen({ children, details, title }: Props) {
  return (
    <main className="flex-auto flex justify-center items-center">
      <ExclamationCircleIcon className="size-12 mb-4" />

      <div>
        <h1>{title}</h1>
        <p className="text-muted mt-3">{details}</p>
      </div>

      {children && (
        <div className="max-w-2xl mt-5">
          <pre>{children}</pre>
        </div>
      )}
    </main>
  );
}

export default ErrorScreen;

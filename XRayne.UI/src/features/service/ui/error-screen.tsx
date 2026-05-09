import type { ReactNode } from "react";

import { ExclamationCircleIcon } from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";

interface Props {
  title: string;
  details: string;
  status: number;
  children?: ReactNode;
}

function ErrorScreen({ children, details, title }: Props) {
  return (
    <main className="flex-auto max-w-4xl flex justify-center items-center">
      <Placeholder>
        <Placeholder.Media>
          <ExclamationCircleIcon className="size-12" />
        </Placeholder.Media>
        <Placeholder.Header>{title}</Placeholder.Header>
        <Placeholder.Subheader>{details}</Placeholder.Subheader>
        {import.meta.env.DEV && children && (
          <div className="mt-5 w-full">
            <pre>{children}</pre>
          </div>
        )}
      </Placeholder>
    </main>
  );
}

export default ErrorScreen;

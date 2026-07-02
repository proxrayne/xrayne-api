import type { ReactNode } from "react";
import { CircleAlertIcon } from "lucide-react";

import Placeholder from "@core/ui/placeholder";
import ColoredIcon from "@core/ui/colored-icon";

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
        <ColoredIcon asChild variant="danger">
          <Placeholder.Media>
            <CircleAlertIcon className="size-12" />
          </Placeholder.Media>
        </ColoredIcon>
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

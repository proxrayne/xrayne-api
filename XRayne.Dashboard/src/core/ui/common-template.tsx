import { ReactNode } from "react";
import { Link } from "react-router";
import { LanguagesIcon } from "lucide-react";

import { urls } from "@core/lib/urls";

import { Button } from "./button";

interface Props {
  children: ReactNode;
}

function CommonTemplate({ children }: Props) {
  return (
    <>
      <header className="w-full sticky top-0 backdrop-blur-2xl">
        <div className="max-w-390 h-16 px-5 flex items-center justify-between gap-x-4 mx-auto">
          <Link to={urls.root()} className="text-2xl font-semibold">
            XRayne
          </Link>
          <Button variant="link">
            <LanguagesIcon className="mr-1.5" />
            English
          </Button>
        </div>
      </header>
      {children}
      <footer className="max-w-390 w-full px-5 pb-10 pt-7 flex items-center justify-between gap-x-3">
        <p className="text-muted-foreground text-xs">
          © {new Date().getFullYear()}. All rights reserved
        </p>

        <div className="flex gap-x-1 text-foreground/90 text-xs">
          <Link to="/docs" className="flex items-center gap-x-1">
            Documentation
          </Link>
          <span className="mx-1">&bull;</span>
          <Link
            to="https://github.com/VanyaKrotov/xrayne-panel"
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-x-1"
          >
            Github
          </Link>
        </div>
      </footer>
    </>
  );
}

export default CommonTemplate;

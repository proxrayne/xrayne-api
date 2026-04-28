import { ComponentProps, useState } from "react";
import { cn, ListBox, Spinner } from "@heroui/react";
import { Link, useLocation } from "react-router";

import GithubIcon from "assets/icons/github.svg?react";

import {
  ArrowRightStartOnRectangleIcon,
  ArrowTopRightOnSquareIcon,
} from "@heroicons/react/16/solid";

import { clearAuthorizationToken } from "@core/api/instance";

import { TOP_NAV } from "../lib/constants";

function NavbarTop() {
  const { pathname } = useLocation();

  return (
    <ListBox selectionMode="single" selectedKeys={["root"]}>
      {TOP_NAV.map(({ icon: Icon, path, title }) => {
        const isSelected = path.includes(pathname);

        return (
          <ListBox.Item
            key={path.toString()}
            textValue={title}
            className={cn({ ["bg-default"]: isSelected })}
            render={({ className }) => (
              <Link to={path} className={className}>
                <Icon className="size-5" />
                {title}
              </Link>
            )}
          />
        );
      })}
    </ListBox>
  );
}

function NavbarBottom() {
  const [isSigningOut, setIsSigningOut] = useState(false);

  return (
    <ListBox className="mb-5">
      <ListBox.Item
        href="https://github.com/VanyaKrotov/XRayna"
        target="_blank"
        key="docs"
      >
        <GithubIcon className="size-5" />
        Github
        <ArrowTopRightOnSquareIcon className="size-4 text-muted ml-auto" />
      </ListBox.Item>
      <ListBox.Item
        isDisabled={isSigningOut}
        render={(props) => (
          <button
            {...(props as ComponentProps<"button">)}
            onClick={() => {
              setIsSigningOut(true);
              clearAuthorizationToken();
            }}
          />
        )}
      >
        {isSigningOut ? (
          <Spinner className="size-5" />
        ) : (
          <ArrowRightStartOnRectangleIcon className="size-5" />
        )}
        Sign out
      </ListBox.Item>
    </ListBox>
  );
}

export default {
  Top: NavbarTop,
  Bottom: NavbarBottom,
};

import { useLayoutEffect, useState } from "react";
import { Link, useLocation } from "react-router";
import { ExternalLinkIcon, LogOutIcon } from "lucide-react";

import GithubIcon from "assets/icons/github.svg?react";

import { clearAuthorizationToken } from "@core/api/instance";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@core/ui/sidebar";
import { Spinner } from "@core/ui/spinner";

import { TOP_NAV } from "./lib/constants";
import Profile from "./ui/profile";

function AppSidebar() {
  const [isSigningOut, setIsSigningOut] = useState(false);

  const { pathname } = useLocation();
  const { setOpenMobile } = useSidebar();

  useLayoutEffect(() => () => setOpenMobile(false), [pathname]);

  return (
    <Sidebar variant="floating" className="sticky top-0">
      <SidebarHeader className="p-4">
        <Profile />
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarMenu>
            {TOP_NAV.map(({ icon: Icon, path, title }) => (
              <SidebarMenuItem key={title}>
                <SidebarMenuButton isActive={path.includes(pathname)} asChild>
                  <Link to={path}>
                    <Icon />
                    {title}
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton asChild>
              <Link to="https://github.com/VanyaKrotov/XRayna" target="_blank">
                <GithubIcon />
                Github
                <ExternalLinkIcon className="ml-auto size-3 text-muted-foreground" />
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
          <SidebarMenuItem>
            <SidebarMenuButton
              disabled={isSigningOut}
              className="cursor-pointer"
              onClick={() => {
                setIsSigningOut(true);
                clearAuthorizationToken();
              }}
            >
              {isSigningOut ? <Spinner /> : <LogOutIcon />}
              Sign out
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  );
}

export default AppSidebar;

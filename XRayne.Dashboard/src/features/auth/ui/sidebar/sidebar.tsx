import { useLayoutEffect } from "react";
import { Link, useLocation } from "react-router";
import { ExternalLinkIcon } from "lucide-react";

import GithubIcon from "assets/icons/github.svg?react";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
  useSidebar,
} from "@core/ui/sidebar";
import Logo from "@core/ui/logo";

import { TOP_NAV } from "./lib/constants";
import Profile from "./ui/profile";

function AppSidebar() {
  const { pathname } = useLocation();
  const { setOpenMobile } = useSidebar();

  useLayoutEffect(() => () => setOpenMobile(false), [pathname]);

  return (
    <Sidebar variant="floating" className="sticky top-0">
      <SidebarHeader className="p-4 flex-row items-center gap-x-2">
        <Logo className="size-6" />
        <span className="font-semibold text-lg">XRayne</span>
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
        </SidebarMenu>
        <SidebarSeparator className="mx-auto" />
        <Profile />
      </SidebarFooter>
    </Sidebar>
  );
}

export default AppSidebar;

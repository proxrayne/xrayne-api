import { createContext } from "react";

import {
  AdjustmentsHorizontalIcon,
  ArrowDownRightIcon,
  ArrowsRightLeftIcon,
  ArrowUpRightIcon,
  PresentationChartBarIcon,
  UserGroupIcon,
} from "@heroicons/react/16/solid";

import { urls } from "@core/lib/urls";

import { SidebarContextType } from "./types";

export const SidebarContext = createContext<SidebarContextType>(
  {} as SidebarContextType,
);

export const TOP_NAV = [
  {
    path: urls.root(),
    title: "Dashboard",
    icon: PresentationChartBarIcon,
  },
  {
    path: urls.users(),
    title: "Users",
    icon: UserGroupIcon,
  },
  {
    path: urls.inbounds(),
    title: "Inbounds",
    icon: ArrowDownRightIcon,
  },
  {
    path: urls.outbounds(),
    title: "Outbounds",
    icon: ArrowUpRightIcon,
  },
  {
    path: urls.routing(),
    title: "Routing",
    icon: ArrowsRightLeftIcon,
  },
  {
    path: urls.settings(),
    title: "Settings",
    icon: AdjustmentsHorizontalIcon,
  },
];

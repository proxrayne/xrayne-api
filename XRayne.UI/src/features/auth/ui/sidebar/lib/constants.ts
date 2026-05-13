import {
  ArrowDownToDotIcon,
  ArrowRightLeftIcon,
  ArrowUpFromDotIcon,
  FileChartPieIcon,
  Settings2Icon,
  UsersIcon,
} from "lucide-react";

import { urls } from "@core/lib/urls";

export const TOP_NAV = [
  {
    path: urls.root(),
    title: "Dashboard",
    icon: FileChartPieIcon,
  },
  {
    path: urls.users(),
    title: "Users",
    icon: UsersIcon,
  },
  {
    path: urls.inbounds(),
    title: "Inbounds",
    icon: ArrowDownToDotIcon,
  },
  {
    path: urls.outbounds(),
    title: "Outbounds",
    icon: ArrowUpFromDotIcon,
  },
  {
    path: urls.routing(),
    title: "Routing",
    icon: ArrowRightLeftIcon,
  },
  {
    path: urls.settings(),
    title: "Settings",
    icon: Settings2Icon,
  },
];

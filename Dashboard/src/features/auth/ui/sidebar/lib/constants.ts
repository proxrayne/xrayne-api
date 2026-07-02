import {
  CpuIcon,
  FileChartPieIcon,
  NetworkIcon,
  ServerIcon,
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
    path: urls.nodes(),
    title: "Nodes",
    icon: NetworkIcon,
  },
  {
    path: urls.hosts(),
    title: "Hosts",
    icon: ServerIcon,
  },
  {
    path: urls.core(),
    title: "Core",
    icon: CpuIcon,
  },
  {
    path: urls.settings(),
    title: "Settings",
    icon: Settings2Icon,
  },
];

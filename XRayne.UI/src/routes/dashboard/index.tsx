import { Route } from "./+types";

import { constructMetadata } from "@core/lib/meta";
import Page from "@core/ui/page";

import { SystemInfo } from "@features/dashboard";

function Dashboard() {
  return (
    <Page>
      <Page.Header>Dashboard</Page.Header>
      <SystemInfo />
    </Page>
  );
}

export function meta({ matches }: Route.MetaArgs) {
  return constructMetadata({ title: "Dashboard" }, matches);
}

export default Dashboard;

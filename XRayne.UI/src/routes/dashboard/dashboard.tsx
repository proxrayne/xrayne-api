import Page from "@core/ui/page";

import { SystemInfo } from "@features/dashboard";

function Dashboard() {
  return (
    <Page>
      <Page.Header>
        <Page.Title>Dashboard</Page.Title>
      </Page.Header>
      <SystemInfo />
    </Page>
  );
}

export default Dashboard;

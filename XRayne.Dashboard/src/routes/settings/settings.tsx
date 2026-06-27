import { AlertCircle } from "lucide-react";

import { Button } from "@core/ui/button";
import Page from "@core/ui/page";
import Placeholder from "@core/ui/placeholder";
import ColoredIcon from "@core/ui/colored-icon";

import { usePanelSettings } from "@features/settings";

import FormProvider from "./ui/form-provider";
import GeneralSection from "./ui/general-section";
import CertsSection from "./ui/certs-section";
import ControlButtons from "./ui/control-buttons";

function Settings() {
  const { settings, pendingRestart, isLoaded, error, refetch } = usePanelSettings();

  return (
    <Page>
      <Page.Header>
        <Page.Title>Panel settings</Page.Title>
        {!error && <Page.Toolbar id={ControlButtons.PortalId} />}
      </Page.Header>

      {(() => {
        if (error) {
          return (
            <Placeholder>
              <ColoredIcon asChild variant="danger">
                <Placeholder.Media>
                  <AlertCircle />
                </Placeholder.Media>
              </ColoredIcon>

              <Placeholder.Header>Failure loading settings</Placeholder.Header>
              <Placeholder.Subheader>Try reload or check logs for details.</Placeholder.Subheader>
              <Placeholder.Actions>
                <Button onClick={() => refetch()}>Try reload</Button>
              </Placeholder.Actions>
            </Placeholder>
          );
        }

        if (!isLoaded) {
          return <div>loading...</div>;
        }

        return (
          <FormProvider settings={settings!}>
            <ControlButtons isLoading={!isLoaded} pendingRestart={pendingRestart} />
            <GeneralSection />
            <CertsSection />
          </FormProvider>
        );
      })()}
    </Page>
  );
}

export default Settings;

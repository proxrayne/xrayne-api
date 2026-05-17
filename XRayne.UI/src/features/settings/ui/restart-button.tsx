import { useState } from "react";
import { RefreshCwIcon } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@core/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@core/ui/dialog";
import { Spinner } from "@core/ui/spinner";

import { pingApi, restartPanel } from "../lib/api";
import { usePanelSettings } from "../lib/query";

const POLL_INTERVAL_MS = 500;
const POLL_TIMEOUT_MS = 30_000;

export function RestartButton() {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [restarting, setRestarting] = useState(false);

  async function doRestart() {
    setConfirmOpen(false);
    setRestarting(true);
    try {
      await restartPanel();
      await waitForApi();
      toast.success("Панель перезапущена");
      await usePanelSettings.invalidate();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Не удалось перезапустить панель";
      toast.error(message);
    } finally {
      setRestarting(false);
    }
  }

  return (
    <>
      <Button
        variant="destructive"
        onClick={() => setConfirmOpen(true)}
        disabled={restarting}
      >
        {restarting && <Spinner className="size-4" />}
        <RefreshCwIcon />
        Перезапуск панели
      </Button>

      <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Перезапустить панель?</DialogTitle>
            <DialogDescription>
              Соединение прервётся на несколько секунд. Несохранённые изменения
              в форме будут потеряны.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmOpen(false)}>
              Отмена
            </Button>
            <Button variant="destructive" onClick={doRestart}>
              Перезапустить
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

async function waitForApi() {
  const deadline = Date.now() + POLL_TIMEOUT_MS;
  while (Date.now() < deadline) {
    try {
      await pingApi();
      return;
    } catch {
      await sleep(POLL_INTERVAL_MS);
    }
  }
  throw new Error("Таймаут ожидания перезапуска");
}

function sleep(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

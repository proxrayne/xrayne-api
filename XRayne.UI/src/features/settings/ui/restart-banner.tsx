import { AlertTriangleIcon } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";

interface RestartBannerProps {
  visible: boolean;
}

export function RestartBanner({ visible }: RestartBannerProps) {
  if (!visible) {
    return null;
  }

  return (
    <Alert className="border-amber-500/40 bg-amber-500/10 text-amber-200">
      <AlertTriangleIcon />
      <AlertTitle>Требуется перезапуск</AlertTitle>
      <AlertDescription>
        Сохраните изменения и перезапустите панель для их применения.
      </AlertDescription>
    </Alert>
  );
}

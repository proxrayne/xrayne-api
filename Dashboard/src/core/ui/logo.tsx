import { forwardRef } from "react";

import LogoIconDark from "assets/icons/logo-x64-dark.svg?react";
import LogoIconLight from "assets/icons/logo-x64-light.svg?react";
import PlainLogoIconDark from "assets/icons/plain-logo-x64-dark.svg?react";
import PlainLogoIconLight from "assets/icons/plain-logo-x64-light.svg?react";

import { useIsDark } from "@core/hooks/use-is-dark";

interface Props extends React.SVGProps<SVGSVGElement> {
  type?: "default" | "plain";
}

const ICONS = {
  default: {
    light: LogoIconDark,
    dark: LogoIconLight,
  },
  plain: {
    light: PlainLogoIconLight,
    dark: PlainLogoIconDark,
  },
} as const;

const Logo = forwardRef<SVGSVGElement, Props>(({ type = "default", ...props }, ref) => {
  const isDark = useIsDark();
  const Icon = ICONS[type][isDark ? "dark" : "light"];

  return <Icon {...props} ref={ref} />;
});

export default Logo;

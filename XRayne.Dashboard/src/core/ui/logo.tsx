import { forwardRef } from "react";

import LogoIconDark from "assets/icons/logo-x64-dark.svg?react";
import LogoIconLight from "assets/icons/logo-x64-light.svg?react";

import { useIsDark } from "@core/hooks/use-is-dark";

const Logo = forwardRef<SVGSVGElement, React.SVGProps<SVGSVGElement>>((props, ref) => {
  const isDark = useIsDark();
  const Icon = isDark ? LogoIconLight : LogoIconDark;

  return <Icon {...props} ref={ref} />;
});

export default Logo;

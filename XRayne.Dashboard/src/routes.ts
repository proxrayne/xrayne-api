import { index, layout, route } from "@react-router/dev/routes";

export default [
  layout("routes/auth/authorized-layout.tsx", [
    route("settings", "routes/settings/index.ts"),
    index("routes/dashboard/index.ts"),
    route("inbounds", "routes/inbounds/index.ts"),
  ]),

  layout("routes/auth/unauthorized-layout.tsx", [
    route("sign-in", "routes/auth/sign-in/index.ts"),
  ]),

  route("*", "features/service/routes/not-found.tsx"),
];

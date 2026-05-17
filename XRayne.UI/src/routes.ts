import { index, layout, route } from "@react-router/dev/routes";

export default [
  layout("routes/auth/authorized-layout.tsx", [
    index("routes/dashboard/index.tsx"),
    route("settings", "routes/settings/index.tsx"),
  ]),

  layout("routes/auth/unauthorized-layout.tsx", [
    route("sign-in", "routes/auth/sign-in/index.ts"),
  ]),

  route("*", "features/service/routes/not-found.tsx"),
];

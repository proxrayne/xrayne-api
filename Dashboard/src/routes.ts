import { index, layout, route } from "@react-router/dev/routes";

export default [
  layout("routes/auth/authorized-layout.tsx", [
    route("settings", "routes/settings/index.ts"),
    route("users", "routes/users/index.ts"),
    route("nodes/:nodeId", "routes/node/index.ts"),
    route("nodes", "routes/nodes/index.ts"),
    route("hosts", "routes/hosts/index.ts"),
    route("core", "routes/core/index.ts"),
    route("inbounds", "routes/inbounds/index.ts"),
    index("routes/dashboard/index.ts"),
  ]),

  layout("routes/auth/unauthorized-layout.tsx", [route("sign-in", "routes/auth/sign-in/index.ts")]),

  route("*", "features/service/routes/not-found.tsx"),
];

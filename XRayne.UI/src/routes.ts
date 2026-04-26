import { index, layout, route } from "@react-router/dev/routes";

export default [
  layout("features/auth/routes/authorized-layout.tsx", [
    index("features/home/routes/main/index.ts"),
  ]),

  layout("features/auth/routes/unauthorized-layout.tsx", [
    route("sign-in", "./features/auth/routes/sign-in/index.ts"),
  ]),

  route("*", "features/service/routes/not-found.tsx"),
];

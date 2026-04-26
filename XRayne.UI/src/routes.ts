import { index, route } from "@react-router/dev/routes";

export default [
  index("./features/home/routes/main/index.ts"),
  route("sign-in", "./features/auth/routes/sign-in/index.ts"),
];

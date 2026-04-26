import { Outlet } from "react-router";

function AuthLayout() {
  return (
    <div className="container">
      <Outlet />
    </div>
  );
}

export default AuthLayout;

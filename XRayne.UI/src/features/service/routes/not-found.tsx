import { Navigate } from "react-router";

import { urls } from "@core/lib/urls";

export default () => <Navigate to={urls.root()} replace />;
